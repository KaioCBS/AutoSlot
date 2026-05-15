using Microsoft.EntityFrameworkCore;
using AutoSlot.Domain.Models;

namespace AutoSlot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<Vaga> Vagas { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Configuracao> Configuracoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Funcionario>().ToTable("funcionarios");
        modelBuilder.Entity<Vaga>().ToTable("vagas");
        modelBuilder.Entity<Reserva>().ToTable("reservas");
        modelBuilder.Entity<Pagamento>().ToTable("pagamentos");
        modelBuilder.Entity<Configuracao>().ToTable("configuracoes");

        modelBuilder.Entity<Funcionario>(e =>
        {
            e.Property(f => f.Id).HasColumnName("id");
            e.Property(f => f.Nome).HasColumnName("nome");
            e.Property(f => f.Email).HasColumnName("email");
            e.Property(f => f.SenhaHash).HasColumnName("senha_hash");
            e.Property(f => f.NivelAcesso).HasColumnName("nivel_acesso");
            e.Property(f => f.Ativo).HasColumnName("ativo");
            e.Property(f => f.CriadoEm).HasColumnName("criado_em");
        });

        modelBuilder.Entity<Vaga>(e =>
        {
            e.Property(v => v.Id).HasColumnName("id");
            e.Property(v => v.Codigo).HasColumnName("codigo");
            e.Property(v => v.Ativa).HasColumnName("ativa");
        });

        modelBuilder.Entity<Configuracao>(e =>
        {
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.TarifaPorHora).HasColumnName("tarifa_por_hora");
            e.Property(c => c.MinutosTolerancia).HasColumnName("minutos_tolerancia");
            e.Property(c => c.AtualizadoEm).HasColumnName("atualizado_em");
        });

        modelBuilder.Entity<Reserva>(e =>
        {
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.VagaId).HasColumnName("vaga_id");
            e.Property(r => r.FuncionarioId).HasColumnName("funcionario_id");
            e.Property(r => r.Entrada).HasColumnName("entrada");
            e.Property(r => r.Saida).HasColumnName("saida");
            e.Property(r => r.CriadoEm).HasColumnName("criado_em");
        });

        modelBuilder.Entity<Pagamento>(e =>
        {
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.ReservaId).HasColumnName("reserva_id");
            e.Property(p => p.FuncionarioId).HasColumnName("funcionario_id");
            e.Property(p => p.ValorCobrado).HasColumnName("valor_cobrado");
            e.Property(p => p.TempoMinutos).HasColumnName("tempo_minutos");
            e.Property(p => p.RegistradoEm).HasColumnName("registrado_em");
        });
    }
}