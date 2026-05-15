namespace AutoSlot.Domain.Models;

public class Vaga
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public bool Ativa { get; set; } = true;
}