using KikasEBonitas.Api.Data;
using KikasEBonitas.Api.Endpoints;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTO DE SERVIÇOS
builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))); 

// Regista a geração da documentação OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// 2. PIPELINE HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); 
}

app.MapGet("/", () => "Bem-vindo à API da Kikas e Bonitas!");

// 3. REGISTO DAS ROTAS MODULARES
app.MapProdutosEndpoints();
app.MapCategoriasEndpoints();

// 4. INICIAR O SERVIDOR
app.Run();
