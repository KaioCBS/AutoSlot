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

    // Criar reserva antecipada
    public async Task<Reserva> CriarReserva(
        int vagaId, int funcionarioId,
        string nomeCliente, string telefoneCliente,
        string placa, string modeloVeiculo,
        DateTime chegadaPrevista, DateTime saidaPrevista)
    {
        if (saidaPrevista <= chegadaPrevista)
            throw new Exception("O horário de saída deve ser maior que o de chegada.");

        var vaga = await _context.Vagas.FindAsync(vagaId);
        if (vaga == null)
            throw new Exception("Vaga não encontrada.");
        if (vaga.Status == "INATIVA")
            throw new Exception("Não é possível reservar uma vaga inativa.");
        if (vaga.Status == "OCUPADA")
            throw new Exception("A vaga está ocupada.");

        // Verificar conflito de horário
        var conflito = await _context.Reservas.AnyAsync(r =>
            r.VagaId == vagaId &&
            (r.Status == "RESERVADA" || r.Status == "OCUPADA") &&
            chegadaPrevista < r.HorarioSaidaPrevisto &&
            saidaPrevista > r.HorarioChegadaPrevisto);

        if (conflito)
            throw new Exception("Conflito de horário: já existe uma reserva nesse período para esta vaga.");

        var reserva = new Reserva
        {
            VagaId = vagaId,
            FuncionarioId = funcionarioId,
            Status = "RESERVADA",
            NomeCliente = nomeCliente,
            TelefoneCliente = telefoneCliente,
            Placa = placa.ToUpper().Replace("-", "").Replace(" ", ""),
            ModeloVeiculo = modeloVeiculo,
            HorarioChegadaPrevisto = chegadaPrevista,
            HorarioSaidaPrevisto = saidaPrevista,
            CriadoEm = DateTime.UtcNow
        };

        vaga.Status = "RESERVADA";
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        return reserva;
    }

    // Listar reservas com filtros
    public async Task<(List<Reserva> reservas, int total)> Listar(
        string? status, string? placa, string? nome,
        DateTime? inicio, DateTime? fim,
        int page, int pageSize)
    {
        var query = _context.Reservas
            .Include(r => r.Vaga)
            .Include(r => r.Funcionario)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(placa))
            query = query.Where(r => r.Placa.Contains(placa.ToUpper()));
        if (!string.IsNullOrEmpty(nome))
            query = query.Where(r => r.NomeCliente.ToLower().Contains(nome.ToLower()));
        if (inicio.HasValue)
            query = query.Where(r => r.HorarioChegadaPrevisto >= inicio.Value);
        if (fim.HasValue)
            query = query.Where(r => r.HorarioChegadaPrevisto <= fim.Value);

        var total = await query.CountAsync();
        var reservas = await query
            .OrderByDescending(r => r.CriadoEm)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (reservas, total);
    }

    // Buscar por ID
    public async Task<Reserva?> BuscarPorId(int id)
    {
        return await _context.Reservas
            .Include(r => r.Vaga)
            .Include(r => r.Funcionario)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    // Editar reserva (só RESERVADA)
    public async Task<Reserva> Editar(
        int id, string nomeCliente, string telefoneCliente,
        string modeloVeiculo, DateTime chegadaPrevista,
        DateTime saidaPrevista, int vagaId)
    {
        var reserva = await _context.Reservas.Include(r => r.Vaga).FirstOrDefaultAsync(r => r.Id == id);
        if (reserva == null)
            throw new Exception("Reserva não encontrada.");
        if (reserva.Status != "RESERVADA")
            throw new Exception("Apenas reservas com status RESERVADA podem ser editadas.");
        if (saidaPrevista <= chegadaPrevista)
            throw new Exception("O horário de saída deve ser maior que o de chegada.");

        // Verificar conflito (excluindo a própria reserva)
        var conflito = await _context.Reservas.AnyAsync(r =>
            r.VagaId == vagaId && r.Id != id &&
            (r.Status == "RESERVADA" || r.Status == "OCUPADA") &&
            chegadaPrevista < r.HorarioSaidaPrevisto &&
            saidaPrevista > r.HorarioChegadaPrevisto);

        if (conflito)
            throw new Exception("Conflito de horário ao editar.");

        // Se trocou de vaga, liberar a antiga
        if (reserva.VagaId != vagaId)
        {
            reserva.Vaga.Status = "LIVRE";
            var novaVaga = await _context.Vagas.FindAsync(vagaId);
            if (novaVaga == null || novaVaga.Status == "INATIVA")
                throw new Exception("Nova vaga não encontrada ou inativa.");
            novaVaga.Status = "RESERVADA";
            reserva.VagaId = vagaId;
        }

        reserva.NomeCliente = nomeCliente;
        reserva.TelefoneCliente = telefoneCliente;
        reserva.ModeloVeiculo = modeloVeiculo;
        reserva.HorarioChegadaPrevisto = chegadaPrevista;
        reserva.HorarioSaidaPrevisto = saidaPrevista;

        await _context.SaveChangesAsync();
        return reserva;
    }

    // Cancelar reserva
    public async Task<Reserva> Cancelar(int id, string? motivo)
    {
        var reserva = await _context.Reservas.Include(r => r.Vaga).FirstOrDefaultAsync(r => r.Id == id);
        if (reserva == null)
            throw new Exception("Reserva não encontrada.");
        if (reserva.Status != "RESERVADA")
            throw new Exception("Apenas reservas com status RESERVADA podem ser canceladas.");

        reserva.Status = "CANCELADA";
        reserva.MotivoCancelamento = motivo;
        reserva.Vaga.Status = "LIVRE";

        await _context.SaveChangesAsync();
        return reserva;
    }

    // Check-in
    public async Task<Reserva> CheckIn(int id)
    {
        var reserva = await _context.Reservas.Include(r => r.Vaga).FirstOrDefaultAsync(r => r.Id == id);
        if (reserva == null)
            throw new Exception("Reserva não encontrada.");
        if (reserva.Status != "RESERVADA")
            throw new Exception("Check-in só é permitido para reservas com status RESERVADA.");
        if (reserva.Vaga.Status == "INATIVA")
            throw new Exception("A vaga está inativa.");

        reserva.Status = "OCUPADA";
        reserva.HorarioChegadaReal = DateTime.UtcNow;
        reserva.Vaga.Status = "OCUPADA";

        await _context.SaveChangesAsync();
        return reserva;
    }

    // Entrada direta (sem reserva prévia)
    public async Task<Reserva> EntradaDireta(
        int vagaId, int funcionarioId,
        string placa, string nomeCliente,
        string telefoneCliente, string modeloVeiculo)
    {
        var vaga = await _context.Vagas.FindAsync(vagaId);
        if (vaga == null)
            throw new Exception("Vaga não encontrada.");
        if (vaga.Status != "LIVRE")
            throw new Exception("A vaga não está livre.");

        var agora = DateTime.UtcNow;
        var reserva = new Reserva
        {
            VagaId = vagaId,
            FuncionarioId = funcionarioId,
            Status = "OCUPADA",
            NomeCliente = nomeCliente,
            TelefoneCliente = telefoneCliente,
            Placa = placa.ToUpper().Replace("-", "").Replace(" ", ""),
            ModeloVeiculo = modeloVeiculo,
            HorarioChegadaPrevisto = agora,
            HorarioSaidaPrevisto = agora.AddHours(2), // estimativa padrão
            HorarioChegadaReal = agora,
            CriadoEm = agora
        };

        vaga.Status = "OCUPADA";
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        return reserva;
    }

    // Checkout — calcula o valor
    public async Task<object> Checkout(int id)
    {
        var reserva = await _context.Reservas.Include(r => r.Vaga).FirstOrDefaultAsync(r => r.Id == id);
        if (reserva == null)
            throw new Exception("Reserva não encontrada.");
        if (reserva.Status != "OCUPADA")
            throw new Exception("Checkout só é permitido para reservas com status OCUPADA.");

        var tarifa = await _context.Tarifas.FirstOrDefaultAsync(t => t.Status == "ATIVA");
        if (tarifa == null)
            throw new Exception("Nenhuma tarifa ativa encontrada.");

        var saida = DateTime.UtcNow;
        var entrada = reserva.HorarioChegadaReal ?? reserva.HorarioChegadaPrevisto;
        var totalMinutos = (int)(saida - entrada).TotalMinutes;
        var minutosTolerancia = tarifa.MinutosTolerancia;

        int horasCobradas = 0;
        decimal valorFinal = 0;

        if (totalMinutos > minutosTolerancia)
        {
            var minutosCobraveis = totalMinutos - minutosTolerancia;
            horasCobradas = (int)Math.Ceiling(minutosCobraveis / 30.0);
            valorFinal = horasCobradas * tarifa.ValorHora;
        }


        return new
        {
            reservaId = reserva.Id,
            vagaId = reserva.VagaId,
            horarioEntrada = entrada,
            horarioSaida = saida,
            minutosTotal = totalMinutos,
            minutosTolerancia,
            horasCobradas,
            valorHora = tarifa.ValorHora,
            valorFinal,
            tarifaId = tarifa.Id
        };
    }

    // Registrar pagamento e finalizar
    public async Task<Pagamento> RegistrarPagamento(
        int reservaId, int funcionarioId,
        string formaPagamento, decimal? valorRecebido)
    {
        var reserva = await _context.Reservas.Include(r => r.Vaga).FirstOrDefaultAsync(r => r.Id == reservaId);
        if (reserva == null)
            throw new Exception("Reserva não encontrada.");
        if (reserva.Status != "OCUPADA")
            throw new Exception("Pagamento só pode ser registrado para reservas OCUPADAS.");

        var pagamentoExistente = await _context.Pagamentos.AnyAsync(p => p.ReservaId == reservaId);
        if (pagamentoExistente)
            throw new Exception("Pagamento já registrado para esta reserva.");

        var tarifa = await _context.Tarifas.FirstOrDefaultAsync(t => t.Status == "ATIVA");
        if (tarifa == null)
            throw new Exception("Nenhuma tarifa ativa encontrada.");

        var saida = DateTime.UtcNow;
        var entrada = reserva.HorarioChegadaReal ?? reserva.HorarioChegadaPrevisto;
        var totalMinutos = (int)(saida - entrada).TotalMinutes;
        decimal valorFinal = 0;

        if (totalMinutos > tarifa.MinutosTolerancia)
        {
            var minutosCobraveis = totalMinutos - tarifa.MinutosTolerancia;
            var horasCobradas = (int)Math.Ceiling(minutosCobraveis / 30.0);
            valorFinal = horasCobradas * tarifa.ValorHora;
        }

        if (formaPagamento == "DINHEIRO")
        {
            if (valorRecebido == null)
                throw new Exception("Valor recebido é obrigatório para pagamento em dinheiro.");
            if (valorRecebido < valorFinal)
                throw new Exception("Valor recebido é menor que o valor cobrado.");
        }

        decimal? troco = formaPagamento == "DINHEIRO" ? valorRecebido - valorFinal : null;

        reserva.HorarioSaidaReal = saida;
        reserva.Status = "FINALIZADA";
        reserva.Vaga.Status = "LIVRE";

        var pagamento = new Pagamento
        {
            ReservaId = reservaId,
            FuncionarioId = funcionarioId,
            TarifaId = tarifa.Id,
            ValorCobrado = valorFinal,
            FormaPagamento = formaPagamento,
            ValorRecebido = valorRecebido,
            Troco = troco,
            TempoMinutos = totalMinutos,
            RegistradoEm = DateTime.UtcNow
        };

        _context.Pagamentos.Add(pagamento);
        await _context.SaveChangesAsync();
        return pagamento;
    }
}