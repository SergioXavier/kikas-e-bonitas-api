using KikasEBonitas.Api;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTO DE SERVIÇOS
// (Reservado para base de dados e dependências no futuro)

var app = builder.Build();

// 2. BASE DE DADOS EM MEMÓRIA (Fase Inicial)
// Uma lista temporária para testarmos os endpoints sem precisar de base de dados real por enquanto.
var produtos = new List<Produto>
{
    new Produto { Id = 1, Nome = "Sérum Facial de Rosa Mosqueta", Categoria = "Cosmética", Preco = 18.50m, Stock = 15 },
    new Produto { Id = 2, Nome = "Bolsa Reutilizável de Algodão Orgânico", Categoria = "Utilitários", Preco = 6.00m, Stock = 40 },
    new Produto { Id = 3, Nome = "Chá de Camomila e Alfazema Biológico", Categoria = "Naturais", Preco = 4.20m, Stock = 25 }
};

// 3. DEFINIÇÃO DAS ROTAS (ENDPOINTS)

// Rota de boas-vindas
app.MapGet("/", () => "Bem-vindo à API da Kikas e Bonitas!");

// Rota GET: Retorna a lista de produtos
app.MapGet("/api/produtos", () => Results.Ok(produtos));

// ROTA GET POR ID: Buscar um produto específico
app.MapGet("/api/produtos/{id:int}", (int id) =>
{
    // Procura na lista o produto com o ID correspondente
    var produto = produtos.FirstOrDefault(p => p.Id == id);
    // Se não encontrar, retorna código HTTP 404 Not Found
    if (produto is null)
    {
        return Results.NotFound(new {Mensagem = $"Produto com ID {id} não foi encontrado."});
    }

    // Se encontrar, retorna 200 OK com o produto
    return Results.Ok(produto);
});

// ROTA POST: Criar um novo produto
app.MapPost("/api/produtos", (Produto novoProduto) =>
{
    // 1. Gerar um novo ID automático (pega no maior ID atual e soma 1)
    novoProduto.Id = produtos.Max(p => p.Id) +1;
    
    // 2. Adicionar o novo produto à nossa lista em memória
    produtos.Add(novoProduto);

    // 3. Retornar resposta HTTP 201 Created com a indicação de onde o item foi criado
    return Results.Created($"/api/produtos/{novoProduto.Id}", novoProduto);
});

// ROTA PUT: Atualizar um produto existente
app.MapPut("/api/produtos/{id:int}", (int id, Produto produtoAtualizado) =>
{
    var produtoExistente = produtos.FirstOrDefault (p => p.Id == id);

    if (produtoExistente is null)
    {
        return Results.NotFound(new {Mensagem = $"Produto com ID {id} não foi encontrado."});
    }

    // Atualiza os dados do produto encontrado
    produtoExistente.Nome = produtoAtualizado.Nome;
    produtoExistente.Categoria = produtoAtualizado.Categoria;
    produtoExistente. Preco = produtoAtualizado.Preco;
    produtoExistente.Stock = produtoAtualizado.Stock;

    return Results.Ok(produtoExistente);
});

// ROTA DELETE: Apagar um produto por ID
app.MapDelete("/api/produtos/{id:int}", (int id) =>
{
    var produto = produtos.FirstOrDefault(p => p.Id == id);

    if (produto is null)
    {
        return Results.NotFound(new {Mensagem = $"Produto com ID {id} não foi encontrado."});
    }

    produtos.Remove(produto);

    // Retorna 204 No Content (sucesso sem corpo de resposta)
    return Results.NoContent();
});

// 4. INICIAR O SERVIDOR
app.Run();
