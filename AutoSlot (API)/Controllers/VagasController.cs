using AutoSlot.Application.Services;
using AutoSlot.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoSlot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VagasController : ControllerBase
{
    private readonly VagasService _vagasService;

    public VagasController(VagasService vagasService)
    {
        _vagasService = vagasService;
    }

    [HttpGet]
    public async Task<IActionResult> ListarTodas()
    {
        var vagas = await _vagasService.ListarTodas();
        return Ok(vagas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var vaga = await _vagasService.BuscarPorId(id);
        if (vaga == null) return NotFound(new { mensagem = "Vaga não encontrada." });
        return Ok(vaga);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] VagaDTO dto)
    {
        try { var vaga = await _vagasService.Criar(dto.Codigo); return Ok(new { mensagem = "Vaga criada com sucesso!", vaga }); }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int id, [FromBody] VagaDTO dto)
    {
        try { var vaga = await _vagasService.Editar(id, dto.Codigo); return Ok(new { mensagem = "Vaga atualizada com sucesso!", vaga }); }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Inativar(int id)
    {
        try { await _vagasService.Inativar(id); return Ok(new { mensagem = "Vaga inativada com sucesso!" }); }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }
}