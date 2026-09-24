using KikasEBonitas.Api.Data;
using KikasEBonitas.Api.Dtos;
using KikasEBonitas.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KikasEBonitas.Api.Endpoints;

public static class ProdutoEndpoints
{
    public static void MapProdutosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/produtos")
                       .WithTags("Produtos");

        // GET: Listar produtos com Filtros de Pesquisa e Paginação
        group.MapGet("/", async (
            string? nome,
            int? categoriaId,
            int pagina = 1,
            int tamanhoPagina = 10,
            AppDbContext db = null!) =>
        {
            // Garantir limites razoáveis para a paginação
            if (pagina < 1) pagina = 1;
            if (tamanhoPagina < 1) tamanhoPagina = 10;
            if (tamanhoPagina > 50) tamanhoPagina = 50; // Limite máximo para evitar sobrecarga

            // 1. Iniciar a query base (IQueryable permite construir a consulta SQL antes de executar)
            var query = db.Produtos.AsQueryable();

            // 2. Aplicar Filtro de Nome (se fornecido)
            if (!string.IsNullOrWhiteSpace(nome))
            {
                query = query.Where(p => p.Nome.ToLower().Contains(nome.ToLower()));
            }

            // 3. Aplicar Filtro de Categoria (se fornecido)
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            // 4. Contar o total de itens que correspondem aos filtros
            var totalItens = await query.CountAsync();

            // 5. Aplicar Paginação (Skip e Take) e projetar para o DTO
            var produtos = await query
                .Include(p => p.Categoria)
                .OrderBy(p => p.Id)
                .Skip((pagina - 1) * tamanhoPagina) // Salta os registos das páginas anteriores
                .Take(tamanhoPagina)               // Pega apenas a quantidade solicitada
                .Select(p => new ProdutoRespostaDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Preco = p.Preco,
                    Stock = p.Stock,
                    CategoriaId = p.CategoriaId,
                    NomeCategoria = p.Categoria != null ? p.Categoria.Nome : "Sem Categoria"
                })
                .ToListAsync();

            // 6. Retornar a resposta envelopada
            var resultado = new ResultadoPaginadoDto<ProdutoRespostaDto>
            {
                Itens = produtos,
                PaginaAtual = pagina,
                TamanhoPagina = tamanhoPagina,
                TotalItens = totalItens
            };

            return Results.Ok(resultado);
        });

        // GET POR ID
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var produto = await db.Produtos
                .Include(p => p.Categoria)
                .Where(p => p.Id == id)
                .Select(p => new ProdutoRespostaDto
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Preco = p.Preco,
                    Stock = p.Stock,
                    CategoriaId = p.CategoriaId,
                    NomeCategoria = p.Categoria != null ? p.Categoria.Nome : "Sem Categoria"
                })
                .FirstOrDefaultAsync();

            return produto is not null 
                ? Results.Ok(produto) 
                : Results.NotFound(new { Mensagem = $"Produto com ID {id} não foi encontrado." });
        });

        // POST: Criar Produto com CategoriaId
        group.MapPost("/", async (CriarProdutoDto dto, AppDbContext db) =>
        {
            // Valida se a Categoria existe na base de dados antes de associar
            var categoriaExiste = await db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
            if (!categoriaExiste)
            {
                return Results.BadRequest(new { Mensagem = $"A categoria com o ID {dto.CategoriaId} não existe." });
            }

            var novoProduto = new Produto
            {
                Nome = dto.Nome,
                Preco = dto.Preco,
                Stock = dto.Stock,
                CategoriaId = dto.CategoriaId
            };

            db.Produtos.Add(novoProduto);
            await db.SaveChangesAsync();

            return Results.Created($"/api/produtos/{novoProduto.Id}", novoProduto);
        })
        .WithParameterValidation();

        // PUT: Atualizar Produto
        group.MapPut("/{id:int}", async (int id, CriarProdutoDto dto, AppDbContext db) =>
        {
            var produtoExistente = await db.Produtos.FindAsync(id);

            if (produtoExistente is null)
            {
                return Results.NotFound(new { Mensagem = $"Produto com ID {id} não foi encontrado." });
            }

            var categoriaExiste = await db.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
            if (!categoriaExiste)
            {
                return Results.BadRequest(new { Mensagem = $"A categoria com o ID {dto.CategoriaId} não existe." });
            }

            produtoExistente.Nome = dto.Nome;
            produtoExistente.Preco = dto.Preco;
            produtoExistente.Stock = dto.Stock;
            produtoExistente.CategoriaId = dto.CategoriaId;

            await db.SaveChangesAsync();

            return Results.Ok(produtoExistente);
        })
        .WithParameterValidation();

        // DELETE: Apagar Produto
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var produto = await db.Produtos.FindAsync(id);

            if (produto is null)
            {
                return Results.NotFound(new { Mensagem = $"Produto com ID {id} não foi encontrado." });
            }

            db.Produtos.Remove(produto);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}