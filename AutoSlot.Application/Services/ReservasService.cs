using AutoSlot.Data;
using AutoSlot.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoSlot.Services;

public class ReservasService
{
    private readonly AppDbContext _context;

    public ReservasService(AppDbContext context)
    {
        _context = context;
    }

    // Registra a entrada de um veículo em uma vaga (RN02: vaga não pode ter duas reservas ativas)
    public async Task<Reserva> RegistrarEntrada(int vagaId, int funcionarioId)
    {
        // Verifica se a vaga existe e está ativa
        var vaga = await _context.Vagas.FindAsync(vagaId);

        if (vaga == null || !vaga.Ativa)
            throw new Exception("Vaga não encontrada ou inativa.");

        // RN02: verifica se a vaga já está ocupada (tem reserva sem saída)
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

    // Registra a saída, calcula o valor e gera o pagamento
    public async Task<Pagamento> RegistrarSaida(int reservaId, int funcionarioId)
    {
        // Busca a reserva ativa
        var reserva = await _context.Reservas
            .Include(r => r.Vaga)
            .FirstOrDefaultAsync(r => r.Id == reservaId && r.Saida == null);

        if (reserva == null)
            throw new Exception("Reserva não encontrada ou veículo já saiu.");

        // Verifica se já existe pagamento para essa reserva (RN03)
        var pagamentoExistente = await _context.Pagamentos
            .AnyAsync(p => p.ReservaId == reservaId);

        if (pagamentoExistente)
            throw new Exception("Pagamento já registrado para esta reserva.");

        // Busca a configuração de tarifa vigente
        var config = await _context.Configuracoes
            .OrderByDescending(c => c.AtualizadoEm)
            .FirstOrDefaultAsync();

        if (config == null)
            throw new Exception("Nenhuma configuração de tarifa encontrada. Cadastre uma tarifa antes de registrar saídas.");

        // Calcula o tempo de permanência
        var saida = DateTime.UtcNow;
        var tempoTotal = saida - reserva.Entrada;
        var totalMinutos = (int)tempoTotal.TotalMinutes;

        // Aplica tolerância (minutos gratuitos antes de cobrar)
        var minutosCobraveis = Math.Max(0, totalMinutos - config.MinutosTolerancia);

        // Calcula o valor: tarifa é por hora, converte minutos para horas
        var valorCobrado = (minutosCobraveis / 60m) * config.TarifaPorHora;
        valorCobrado = Math.Round(valorCobrado, 2);

        // Registra a saída na reserva
        reserva.Saida = saida;

        // Cria o pagamento
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

    // Lista todas as reservas ativas (veículos ainda no estacionamento)
    public async Task<List<object>> ListarAtivas()
    {
        var reservas = await _context.Reservas
            .Include(r => r.Vaga)
            .Include(r => r.Funcionario)
            .Where(r => r.Saida == null)
            .OrderBy(r => r.Entrada)
            .ToListAsync();

        // Retorna um objeto com dados relevantes e tempo decorrido
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