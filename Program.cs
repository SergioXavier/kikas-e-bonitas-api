using System.Text;
using KikasEBonitas.Api.Data;
using KikasEBonitas.Api.Endpoints;
using KikasEBonitas.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// 1. REGISTO DE SERVIÇOS
builder.Services.AddDbContext<AppDbContext>(options =>
   options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))); 

// Regista a geração da documentação OpenAPI
builder.Services.AddOpenApi();

// REGISTO DO GLOBAL EXCEPTION HANDLER
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// CONFIGURAÇÃO DO JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ATIVAR AUTENTICAÇÃO E AUTORIZAÇÃO NO PIPELINE
app.UseAuthentication();
app.UseAuthorization();

// ATIVAR O MIDDLEWARE DE TRATAMENTO DE ERROS (Deve vir no início do pipeline HTTP!)
app.UseExceptionHandler();

// 2. PIPELINE HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); 
}

app.MapGet("/", () => "Bem-vindo à API da Kikas e Bonitas!");

// 3. REGISTO DAS ROTAS MODULARES
app.MapAuthEndpoints(builder.Configuration);
app.MapProdutosEndpoints();
app.MapCategoriasEndpoints();

// 4. INICIAR O SERVIDOR
app.Run();
