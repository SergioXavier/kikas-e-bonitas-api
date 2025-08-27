namespace KikasBonitas.Api.Models;

public class Payment
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentStatus Status {get; set;} = PaymentStatus.Pending;
    public PaymentMethod Method { get; set; }
}