namespace KikasBonitas.Api.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Address { get; set; }

    public List<Order> Orders { get; set; } = new();
}