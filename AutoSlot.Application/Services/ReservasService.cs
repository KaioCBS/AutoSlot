using AutoSlot.Domain.Models;
using AutoSlot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoSlot.Application.Services;

public class ReservasService
{
    private readonly AppDbContext _context;

    public ReservasService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reserva> RegistrarEntrada(int vagaId, int funcionarioId)
    {
        var vaga = await _context.Vagas.FindAsync(vagaId);

        if (vaga == null || !vaga.Ativa)
            throw new Exception("Vaga não encontrada ou inativa.");

        var vagaOcupada = await _context.Reservas
            .AnyAsync(r => r.VagaId == vagaId && r.Saida == null);

        if (vagaOcupada)
            throw new Exception($"A vaga '{vaga.Codigo}' já está ocupada.");

        var reserva = new Reserva
        {
            VagaId = vagaId,
            FuncionarioId = funcionarioId,
            Entrada = DateTime.UtcNow,
            CriadoEm = DateTime.UtcNow
        };

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        return reserva;
    }

    public async Task<Pagamento> RegistrarSaida(int reservaId, int funcionarioId)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Vaga)
            .FirstOrDefaultAsync(r => r.Id == reservaId && r.Saida == null);

        if (reserva == null)
            throw new Exception("Reserva não encontrada ou veículo já saiu.");

        var pagamentoExistente = await _context.Pagamentos
            .AnyAsync(p => p.ReservaId == reservaId);

        if (pagamentoExistente)
            throw new Exception("Pagamento já registrado para esta reserva.");

        var config = await _context.Configuracoes
            .OrderByDescending(c => c.AtualizadoEm)
            .FirstOrDefaultAsync();

        if (config == null)
            throw new Exception("Nenhuma configuração de tarifa encontrada.");

        var saida = DateTime.UtcNow;
        var totalMinutos = (int)(saida - reserva.Entrada).TotalMinutes;
        var minutosCobraveis = Math.Max(0, totalMinutos - config.MinutosTolerancia);
        var valorCobrado = Math.Round((minutosCobraveis / 60m) * config.TarifaPorHora, 2);

        reserva.Saida = saida;

        var pagamento = new Pagamento
        {
            ReservaId = reservaId,
            FuncionarioId = funcionarioId,
            ValorCobrado = valorCobrado,
            TempoMinutos = totalMinutos,
            RegistradoEm = DateTime.UtcNow
        };

        _context.Pagamentos.Add(pagamento);
        await _context.SaveChangesAsync();
        return pagamento;
    }

    public async Task<List<object>> ListarAtivas()
    {
        var reservas = await _context.Reservas
            .Include(r => r.Vaga)
            .Include(r => r.Funcionario)
            .Where(r => r.Saida == null)
            .OrderBy(r => r.Entrada)
            .ToListAsync();

        return reservas.Select(r => (object)new
        {
            reservaId = r.Id,
            vagaId = r.VagaId,
            vagaCodigo = r.Vaga.Codigo,
            funcionario = r.Funcionario.Nome,
            entrada = r.Entrada,
            tempoDecorrido = $"{(int)(DateTime.UtcNow - r.Entrada).TotalHours}h {(int)(DateTime.UtcNow - r.Entrada).Minutes}min"
        }).ToList();
    }
}