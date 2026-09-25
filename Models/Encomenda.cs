namespace KikasEBonitas.Api.Models;

public class Encomenda
{
    public int Id {get; set; }
    public DateTime DataCriacao {get; set; } = DateTime.UtcNow;
    public string NomeCliente {get; set; } = string.Empty;
    public string EmailCliente {get; set; } = string.Empty;
    public decimal ValorTotal {get; set; }
    public StatusEncomenda Status {get; set; } = StatusEncomenda.Pendente;

    public List<ItemEncomenda> Itens {get; set; } = new();
}

public enum StatusEncomenda
{
    Pendente,
    Paga,
    Enviada,
    Cancelada
}