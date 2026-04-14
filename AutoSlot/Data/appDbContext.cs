using Microsoft.EntityFrameworkCore;
using AutoSlot.Models;

namespace AutoSlot.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<Vaga> Vagas { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Configuracao> Configuracoes { get; set; }
}