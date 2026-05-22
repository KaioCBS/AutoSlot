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

    // GET api/configuracoes/tarifa-ativa
    [HttpGet("tarifa-ativa")]
    public async Task<IActionResult> ObterTarifaAtiva()
    {
        var tarifa = await _configuracoesService.ObterTarifaAtiva();
        if (tarifa == null)
            return NotFound(new { mensagem = "Nenhuma tarifa ativa encontrada." });
        return Ok(tarifa);
    }

    // GET api/configuracoes/tarifas?status=ATIVA
    [HttpGet("tarifas")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListarTarifas([FromQuery] string? status = null)
    {
        var tarifas = await _configuracoesService.ListarTarifas(status);
        return Ok(new { tarifas });
    }

    // POST api/configuracoes/tarifas
    [HttpPost("tarifas")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CriarTarifa([FromBody] TarifaDTO dto)
    {
        try
        {
            var tarifa = await _configuracoesService.CriarTarifa(
                dto.ValorHora, dto.MinutosTolerancia, dto.DataVigencia, dto.Status);
            return StatusCode(201, new { mensagem = "Tarifa criada com sucesso!", tarifa });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    // PATCH api/configuracoes/tarifas/5/ativar
    [HttpPatch("tarifas/{id}/ativar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AtivarTarifa(int id)
    {
        try
        {
            var tarifa = await _configuracoesService.AtivarTarifa(id);
            return Ok(new { mensagem = "Tarifa ativada com sucesso!", tarifa });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    // PATCH api/configuracoes/tarifas/5/inativar
    [HttpPatch("tarifas/{id}/inativar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> InativarTarifa(int id)
    {
        try
        {
            var tarifa = await _configuracoesService.InativarTarifa(id);
            return Ok(new { mensagem = "Tarifa inativada com sucesso!", tarifa });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }
}