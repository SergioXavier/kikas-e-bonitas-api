using System.ComponentModel.DataAnnotations;

namespace KikasEBonitas.Api.Dtos;

public class ItemEncomendaCriacaoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um produto válido.")]
    public int ProdutoId {get; set; }

    [Range(1, 100, ErrorMessage = "A quantidade deve ser entre 1 e 100 unidades.")]
    public int Quantidade {get; set; }
}

public class CriarEncomendaDto
{
    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    public string NomeCliente {get; set; } = string.Empty;

    [Required, EmailAddress(ErrorMessage = "Forneça um email válido.")]
    public string EmailCliente {get; set; } = string.Empty;

    [Required, MinLength(1, ErrorMessage = "A encomenda deve conter pelo menos 1 item.")]
    public List<ItemEncomendaCriacaoDto> Itens {get; set; } = new();
    
}

public class ItemEncomendaRespostaDto
{
    public int ProdutoId {get; set; }
    public string NomeProduto {get; set; } = string.Empty;
    public int Quantidade {get; set; }
    public decimal PrecoUnitario {get; set; }
    public decimal Subtotal => Quantidade * PrecoUnitario;
}

public class EncomendaRespostaDto
{
    public int Id {get; set; }
    public DateTime DataCriacao {get; set; }
    public string NomeCliente {get; set; } = string.Empty;
    public string EmailCliente {get; set; } = string.Empty;
    public string Status {get; set; } = string.Empty;
    public decimal ValorTotal {get; set; }
    public List<ItemEncomendaRespostaDto> Itens {get; set; } = new();
}