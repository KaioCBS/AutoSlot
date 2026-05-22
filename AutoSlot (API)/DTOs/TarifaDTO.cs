namespace AutoSlot.DTOs;

public class TarifaDTO
{
    public decimal ValorHora { get; set; }
    public int MinutosTolerancia { get; set; }
    public DateTime DataVigencia { get; set; }
    public string Status { get; set; } = "INATIVA";
}