namespace AutoSlot.Models;

public class Pagamento
{
    public int Id { get; set; }
    public int ReservaId { get; set; }
    public Reserva Reserva { get; set; } = null!;
    public int FuncionarioId { get; set; }
    public Funcionario Funcionario { get; set; } = null!;
    public decimal ValorCobrado { get; set; }
    public int TempoMinutos { get; set; }
    public DateTime RegistradoEm { get; set; } = DateTime.Now;
}