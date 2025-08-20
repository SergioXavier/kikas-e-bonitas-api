namespace KikasBonitas.Api.Models;

public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int? CategoryId { get; set; }
    public string? Sizes { get; set; }
    public string? Colors { get; set; }
    public string? ImageUrl { get; set; }

    public Category? Category { get; set; }
}
