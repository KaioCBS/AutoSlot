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

    public async Task<Configuracao?> Obter()
    {
        return await _context.Configuracoes
            .OrderByDescending(c => c.AtualizadoEm)
            .FirstOrDefaultAsync();
    }

    public async Task<Configuracao> Atualizar(decimal tarifaPorHora, int minutosTolerancia)
    {
        if (tarifaPorHora <= 0)
            throw new Exception("A tarifa por hora deve ser maior que zero.");

        if (minutosTolerancia < 0)
            throw new Exception("Os minutos de tolerância não podem ser negativos.");

        var config = await _context.Configuracoes.FirstOrDefaultAsync();

        if (config == null)
        {
            config = new Configuracao
            {
                TarifaPorHora = tarifaPorHora,
                MinutosTolerancia = minutosTolerancia,
                AtualizadoEm = DateTime.UtcNow
            };
            _context.Configuracoes.Add(config);
        }
        else
        {
            config.TarifaPorHora = tarifaPorHora;
            config.MinutosTolerancia = minutosTolerancia;
            config.AtualizadoEm = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return config;
    }
}