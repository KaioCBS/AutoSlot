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

    // GET api/vagas?status=LIVRE
    [HttpGet]
    public async Task<IActionResult> ListarTodas([FromQuery] string? status = null)
    {
        var vagas = await _vagasService.ListarTodas(status);
        return Ok(new { vagas });
    }

    // GET api/vagas/mapa
    [HttpGet("mapa")]
    public async Task<IActionResult> Mapa()
    {
        var vagas = await _vagasService.ListarMapa();
        return Ok(new { vagas });
    }

    // GET api/vagas/5
    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var vaga = await _vagasService.BuscarPorId(id);
        if (vaga == null)
            return NotFound(new { mensagem = "Vaga não encontrada." });
        return Ok(vaga);
    }

    // POST api/vagas
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Criar([FromBody] VagaDTO dto)
    {
        try
        {
            var vaga = await _vagasService.Criar(dto.Codigo, dto.TipoVaga, dto.PosicaoX, dto.PosicaoY);
            return StatusCode(201, new { mensagem = "Vaga criada com sucesso!", vaga });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    // PUT api/vagas/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(int id, [FromBody] VagaDTO dto)
    {
        try
        {
            var vaga = await _vagasService.Editar(id, dto.Codigo, dto.TipoVaga, dto.PosicaoX, dto.PosicaoY);
            return Ok(new { mensagem = "Vaga atualizada com sucesso!", vaga });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    // PATCH api/vagas/5/inativar
    [HttpPatch("{id}/inativar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Inativar(int id)
    {
        try
        {
            await _vagasService.Inativar(id);
            return Ok(new { mensagem = "Vaga inativada com sucesso!" });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    // PATCH api/vagas/5/reativar
    [HttpPatch("{id}/reativar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reativar(int id)
    {
        try
        {
            await _vagasService.Reativar(id);
            return Ok(new { mensagem = "Vaga reativada com sucesso!" });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }

    // DELETE api/vagas/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id)
    {
        try
        {
            await _vagasService.Excluir(id);
            return Ok(new { mensagem = "Vaga excluída com sucesso!" });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }
}