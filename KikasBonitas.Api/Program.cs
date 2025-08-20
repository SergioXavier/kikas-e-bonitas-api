using KikasBonitas.Api.Data;
using KikasBonitas.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB (SQLite)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=app.db"));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (liberal para dev; restringe em produção)
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

// criar base de dados no arranque (dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Saúde
app.MapGet("/", () => "Kikas e Bonitas API v1");

// Produtos CRUD
var products = app.MapGroup("/api/products");

products.MapGet("/", async (AppDbContext db) =>
    await db.Products.AsNoTracking().ToListAsync());

products.MapGet("/{id:int}", async (int id, AppDbContext db) =>
    await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id) is { } p
        ? Results.Ok(p) : Results.NotFound());

products.MapPost("/", async (Product input, AppDbContext db) =>
{
    db.Products.Add(input);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{input.ProductId}", input);
});

products.MapPut("/{id:int}", async (int id, Product input, AppDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();

    p.Name = input.Name;
    p.Description = input.Description;
    p.Price = input.Price;
    p.Stock = input.Stock;
    p.CategoryId = input.CategoryId;
    p.Sizes = input.Sizes;
    p.Colors = input.Colors;
    p.ImageUrl = input.ImageUrl;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

products.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();

    db.Products.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
