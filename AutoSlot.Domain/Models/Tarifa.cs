namespace AutoSlot.Domain.Models;

public class Tarifa
{
    public int Id { get; set; }
    public decimal ValorHora { get; set; }
    public int MinutosTolerancia { get; set; }
    public DateTime DataVigencia { get; set; }
    public string Status { get; set; } = "INATIVA"; // ATIVA, INATIVA
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}