namespace KikasBonitas.Api.Models;

    public class Order
    {
        public int OrderId { get; set; }   // ou Id, se preferires padronizar
        public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // relação 1:N -> um pedido pode ter vários items
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // relação 1:1 -> um pedido pode ter um pagamento
    public Payment Payment { get; set; } = null!;
    }
