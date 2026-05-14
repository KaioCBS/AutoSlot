using AutoSlot.Data;
using AutoSlot.DTOs;
using AutoSlot.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoSlot.Services;

public class ConfiguracoesService
{
    private readonly AppDbContext _context;

    public ConfiguracoesService(AppDbContext context)
    {
        _context = context;
    }

    // Retorna a configuração mais recente
    public async Task<Configuracao?> Obter()
    {
        return await _context.Configuracoes
            .OrderByDescending(c => c.AtualizadoEm)
            .FirstOrDefaultAsync();
    }

    // Cria ou atualiza a configuração de tarifa e tolerância
    public async Task<Configuracao> Atualizar(ConfiguracaoDTO dto)
    {
        if (dto.TarifaPorHora <= 0)
            throw new Exception("A tarifa por hora deve ser maior que zero.");

        if (dto.MinutosTolerancia < 0)
            throw new Exception("Os minutos de tolerância não podem ser negativos.");

        // Busca configuração existente para atualizar, ou cria uma nova
        var config = await _context.Configuracoes.FirstOrDefaultAsync();

        if (config == null)
        {
            config = new Configuracao
            {
                TarifaPorHora = dto.TarifaPorHora,
                MinutosTolerancia = dto.MinutosTolerancia,
                AtualizadoEm = DateTime.UtcNow
            };
            _context.Configuracoes.Add(config);
        }
        else
        {
            config.TarifaPorHora = dto.TarifaPorHora;
            config.MinutosTolerancia = dto.MinutosTolerancia;
            config.AtualizadoEm = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return config;
    }
}