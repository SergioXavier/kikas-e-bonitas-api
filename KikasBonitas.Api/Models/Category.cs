namespace KikasBonitas.Api.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public List<Product> Products { get; set; } = new();
}