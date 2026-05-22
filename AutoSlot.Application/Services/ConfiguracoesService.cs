using AutoSlot.Domain.Models;
using AutoSlot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoSlot.Application.Services;

public class ConfiguracoesService
{
    private readonly AppDbContext _context;

    public ConfiguracoesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tarifa?> ObterTarifaAtiva()
    {
        return await _context.Tarifas
            .FirstOrDefaultAsync(t => t.Status == "ATIVA");
    }

    public async Task<List<Tarifa>> ListarTarifas(string? status = null)
    {
        var query = _context.Tarifas.AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);
        return await query.OrderByDescending(t => t.CriadoEm).ToListAsync();
    }

    public async Task<Tarifa> CriarTarifa(decimal valorHora, int minutosTolerancia, DateTime dataVigencia, string status)
    {
        if (valorHora <= 0)
            throw new Exception("O valor por hora deve ser maior que zero.");
        if (minutosTolerancia < 0)
            throw new Exception("Os minutos de tolerância não podem ser negativos.");

        if (status == "ATIVA")
            await InativarTarifaAtual();

        var tarifa = new Tarifa
        {
            ValorHora = valorHora,
            MinutosTolerancia = minutosTolerancia,
            DataVigencia = dataVigencia,
            Status = status,
            CriadoEm = DateTime.UtcNow
        };

        _context.Tarifas.Add(tarifa);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var mensagem = ex.InnerException?.InnerException?.Message
                        ?? ex.InnerException?.Message
                        ?? ex.Message;
            throw new Exception(mensagem);
        }

        return tarifa;
    }

    public async Task<Tarifa> AtivarTarifa(int id)
    {
        var tarifa = await _context.Tarifas.FindAsync(id);
        if (tarifa == null)
            throw new Exception("Tarifa não encontrada.");

        await InativarTarifaAtual();

        tarifa.Status = "ATIVA";
        await _context.SaveChangesAsync();
        return tarifa;
    }

    public async Task<Tarifa> InativarTarifa(int id)
    {
        var tarifa = await _context.Tarifas.FindAsync(id);
        if (tarifa == null)
            throw new Exception("Tarifa não encontrada.");

        var totalAtivas = await _context.Tarifas.CountAsync(t => t.Status == "ATIVA");
        if (tarifa.Status == "ATIVA" && totalAtivas <= 1)
            throw new Exception("Não é possível inativar a única tarifa ativa do sistema.");

        tarifa.Status = "INATIVA";
        await _context.SaveChangesAsync();
        return tarifa;
    }

    private async Task InativarTarifaAtual()
    {
        var tarifaAtiva = await _context.Tarifas.FirstOrDefaultAsync(t => t.Status == "ATIVA");
        if (tarifaAtiva != null)
        {
            tarifaAtiva.Status = "INATIVA";
            await _context.SaveChangesAsync();
        }
    }
}