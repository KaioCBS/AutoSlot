namespace AutoSlot.Models;

public class Reserva
{
    public int Id { get; set; }
    public int VagaId { get; set; }
    public Vaga Vaga { get; set; } = null!;
    public int FuncionarioId { get; set; }
    public Funcionario Funcionario { get; set; } = null!;
    public DateTime Entrada { get; set; }
    public DateTime? Saida { get; set; } // null = ainda ocupada
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}