using AutoSlot.Application.Services;
using AutoSlot.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoSlot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracoesController : ControllerBase
{
    private readonly ConfiguracoesService _configuracoesService;

    public ConfiguracoesController(ConfiguracoesService configuracoesService)
    {
        _configuracoesService = configuracoesService;
    }

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var config = await _configuracoesService.Obter();
        if (config == null) return NotFound(new { mensagem = "Nenhuma configuração cadastrada ainda." });
        return Ok(new { id = config.Id, tarifaPorHora = config.TarifaPorHora, minutosTolerancia = config.MinutosTolerancia, atualizadoEm = config.AtualizadoEm });
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Atualizar([FromBody] ConfiguracaoDTO dto)
    {
        try
        {
            var config = await _configuracoesService.Atualizar(dto.TarifaPorHora, dto.MinutosTolerancia);
            return Ok(new { mensagem = "Configurações atualizadas com sucesso!", tarifaPorHora = config.TarifaPorHora, minutosTolerancia = config.MinutosTolerancia, atualizadoEm = config.AtualizadoEm });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }
}