using AutoSlot.DTOs;
using AutoSlot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoSlot.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // todas as rotas exigem login
public class ReservasController : ControllerBase
{
    private readonly ReservasService _reservasService;

    public ReservasController(ReservasService reservasService)
    {
        _reservasService = reservasService;
    }

    // POST api/reservas/entrada
    // Registra a entrada de um veículo em uma vaga
    [HttpPost("entrada")]
    public async Task<IActionResult> RegistrarEntrada([FromBody] EntradaDTO dto)
    {
        try
        {
            var funcionarioId = ObterFuncionarioId();
            var reserva = await _reservasService.RegistrarEntrada(dto.VagaId, funcionarioId);

            return Ok(new
            {
                mensagem = "Entrada registrada com sucesso!",
                reservaId = reserva.Id,
                vagaId = reserva.VagaId,
                entrada = reserva.Entrada,
                funcionarioId = reserva.FuncionarioId
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // POST api/reservas/saida/{id}
    // Registra a saída, calcula o tempo e gera o pagamento automaticamente
    [HttpPost("saida/{id}")]
    public async Task<IActionResult> RegistrarSaida(int id)
    {
        try
        {
            var funcionarioId = ObterFuncionarioId();
            var pagamento = await _reservasService.RegistrarSaida(id, funcionarioId);

            return Ok(new
            {
                mensagem = "Saída registrada e pagamento gerado com sucesso!",
                pagamentoId = pagamento.Id,
                reservaId = pagamento.ReservaId,
                tempoMinutos = pagamento.TempoMinutos,
                valorCobrado = pagamento.ValorCobrado,
                registradoEm = pagamento.RegistradoEm
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // GET api/reservas/ativas
    // Lista todos os veículos que ainda estão no estacionamento
    [HttpGet("ativas")]
    public async Task<IActionResult> ListarAtivas()
    {
        var reservas = await _reservasService.ListarAtivas();
        return Ok(reservas);
    }

    // Lê o ID do funcionário logado direto do token JWT
    private int ObterFuncionarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int id))
            throw new Exception("Funcionário não identificado no token.");

        return id;
    }
}