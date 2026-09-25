using KikasEBonitas.Api.Data;
using KikasEBonitas.Api.Dtos;
using KikasEBonitas.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KikasEBonitas.Api.Endpoints;

public static class EncomendaEndpoints
{
    public static RouteGroupBuilder MapEncomendaEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/encomendas")
                          .WithTags("Encomendas");

       // POST: /api/encomendas
        group.MapPost("/", async (CriarEncomendaDto dto, AppDbContext db) =>
        {
            if (dto.Itens == null || dto.Itens.Count == 0)
            {
                return Results.BadRequest(new { mensagem = "A encomenda deve conter pelo menos um item." });
            }

            // Usar uma transação para garantir consistência (Encomenda + Stock)
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                var idsProdutos = dto.Itens.Select(i => i.ProdutoId).Distinct().ToList();
                var produtosDb = await db.Produtos
                    .Where(p => idsProdutos.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                // Validar existência e stock disponível
                foreach (var item in dto.Itens)
                {
                    if (!produtosDb.TryGetValue(item.ProdutoId, out var produto))
                    {
                        return Results.BadRequest(new { mensagem = $"Produto com ID {item.ProdutoId} não foi encontrado." });
                    }

                    if (produto.Stock < item.Quantidade)
                    {
                        return Results.BadRequest(new { mensagem = $"Stock insuficiente para o produto '{produto.Nome}'. Disponível: {produto.Stock}, requerido: {item.Quantidade}." });
                    }
                }

                var encomenda = new Encomenda
                {
                    NomeCliente = dto.NomeCliente,
                    EmailCliente = dto.EmailCliente,
                    DataCriacao = DateTime.UtcNow,
                    Status = StatusEncomenda.Pendente,
                    Itens = new List<ItemEncomenda>()
                };

                decimal valorTotal = 0m;

                foreach (var itemDto in dto.Itens)
                {
                    var produto = produtosDb[itemDto.ProdutoId];
                    decimal precoUnitario = produto.Preco;

                    valorTotal += precoUnitario * itemDto.Quantidade;

                    // Abater o stock
                    produto.Stock -= itemDto.Quantidade;

                    encomenda.Itens.Add(new ItemEncomenda
                    {
                        ProdutoId = produto.Id,
                        Quantidade = itemDto.Quantidade,
                        PrecoUnitario = precoUnitario
                    });
                }

                encomenda.ValorTotal = valorTotal;

                db.Encomendas.Add(encomenda);
                await db.SaveChangesAsync();
                
                // Confirmar a transação com sucesso
                await transaction.CommitAsync();

                var respostaDto = new EncomendaRespostaDto
                {
                    Id = encomenda.Id,
                    DataCriacao = encomenda.DataCriacao,
                    NomeCliente = encomenda.NomeCliente,
                    EmailCliente = encomenda.EmailCliente,
                    Status = encomenda.Status.ToString(),
                    ValorTotal = encomenda.ValorTotal,
                    Itens = encomenda.Itens.Select(i => new ItemEncomendaRespostaDto
                    {
                        ProdutoId = i.ProdutoId,
                        NomeProduto = produtosDb[i.ProdutoId].Nome,
                        Quantidade = i.Quantidade,
                        PrecoUnitario = i.PrecoUnitario
                    }).ToList()
                };

                return Results.Created($"/api/encomendas/{encomenda.Id}", respostaDto);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });

        // GET: /api/encomendas
        group.MapGet("/", async (AppDbContext db) =>
        {
            var encomendas = await db.Encomendas
                .Include(e => e.Itens)
                .ThenInclude(i => i.Produto)
                .AsNoTracking()
                .ToListAsync();

            var resposta = encomendas.Select(e => new EncomendaRespostaDto
            {
                Id = e.Id,
                DataCriacao = e.DataCriacao,
                NomeCliente = e.NomeCliente,
                EmailCliente = e.EmailCliente,
                Status = e.Status.ToString(),
                ValorTotal = e.ValorTotal,
                Itens = e.Itens.Select(i => new ItemEncomendaRespostaDto
                {
                    ProdutoId = i.ProdutoId,
                    NomeProduto = i.Produto?.Nome ?? "Produto Desconhecido",
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario
                }).ToList()
            }).ToList();

            return Results.Ok(resposta);
        });

        // GET: /api/encomendas/{id}
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var e = await db.Encomendas
                .Include(enc => enc.Itens)
                .ThenInclude(i => i.Produto)
                .AsNoTracking()
                .FirstOrDefaultAsync(enc => enc.Id == id);

            if (e == null)
            {
                return Results.NotFound(new
                {
                    mensagem = "Encomenda não encontrada."
                });
            }

            var resposta = new EncomendaRespostaDto
            {
                Id = e.Id,
                DataCriacao = e.DataCriacao,
                NomeCliente = e.NomeCliente,
                EmailCliente = e.EmailCliente,
                Status = e.Status.ToString(),
                ValorTotal = e.ValorTotal,
                Itens = e.Itens.Select(i => new ItemEncomendaRespostaDto
                {
                    ProdutoId = i.ProdutoId,
                    NomeProduto = i.Produto?.Nome ?? "Produto Desconhecido",
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario
                }).ToList()
            };

            return Results.Ok(resposta);
        });

        return group;
    }
}