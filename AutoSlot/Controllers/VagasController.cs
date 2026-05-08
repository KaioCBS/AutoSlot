using AutoSlot.DTOs;
using AutoSlot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoSlot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // todas as rotas exigem login
public class VagasController : ControllerBase
{
    private readonly VagasService _vagasService;

    public VagasController(VagasService vagasService)
    {
        _vagasService = vagasService;
    }

    // GET api/vagas
    [HttpGet]
    public async Task<IActionResult> ListarTodas()
    {
        var vagas = await _vagasService.ListarTodas();
        return Ok(vagas);
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
    [Authorize(Roles = "Admin")] // só Admin pode criar vagas
    public async Task<IActionResult> Criar([FromBody] VagaDTO dto)
    {
        try
        {
            var vaga = await _vagasService.Criar(dto);
            return Ok(new { mensagem = "Vaga criada com sucesso!", vaga });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // PUT api/vagas/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")] // só Admin pode editar vagas
    public async Task<IActionResult> Editar(int id, [FromBody] VagaDTO dto)
    {
        try
        {
            var vaga = await _vagasService.Editar(id, dto);
            return Ok(new { mensagem = "Vaga atualizada com sucesso!", vaga });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // DELETE api/vagas/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // só Admin pode inativar vagas
    public async Task<IActionResult> Inativar(int id)
    {
        try
        {
            await _vagasService.Inativar(id);
            return Ok(new { mensagem = "Vaga inativada com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}