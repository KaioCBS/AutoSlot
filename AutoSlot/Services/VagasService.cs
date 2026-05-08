using AutoSlot.Data;
using AutoSlot.DTOs;
using AutoSlot.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoSlot.Services;

public class VagasService
{
    private readonly AppDbContext _context;

    public VagasService(AppDbContext context)
    {
        _context = context;
    }

    // Lista todas as vagas ativas
    public async Task<List<Vaga>> ListarTodas()
    {
        return await _context.Vagas
            .Where(v => v.Ativa)
            .ToListAsync();
    }

    // Busca uma vaga pelo ID
    public async Task<Vaga?> BuscarPorId(int id)
    {
        return await _context.Vagas
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    // Cria uma nova vaga
    public async Task<Vaga> Criar(VagaDTO dto)
    {
        // Verifica se já existe uma vaga com esse código
        var existe = await _context.Vagas
            .AnyAsync(v => v.Codigo == dto.Codigo);

        if (existe)
            throw new Exception($"Já existe uma vaga com o código '{dto.Codigo}'.");

        var vaga = new Vaga
        {
            Codigo = dto.Codigo,
            Ativa = true
        };

        _context.Vagas.Add(vaga);
        await _context.SaveChangesAsync();

        return vaga;
    }

    // Edita o código de uma vaga
    public async Task<Vaga> Editar(int id, VagaDTO dto)
    {
        var vaga = await _context.Vagas.FindAsync(id);

        if (vaga == null)
            throw new Exception("Vaga não encontrada.");

        // Verifica se o novo código já está em uso por outra vaga
        var codigoEmUso = await _context.Vagas
            .AnyAsync(v => v.Codigo == dto.Codigo && v.Id != id);

        if (codigoEmUso)
            throw new Exception($"Já existe uma vaga com o código '{dto.Codigo}'.");

        vaga.Codigo = dto.Codigo;
        await _context.SaveChangesAsync();

        return vaga;
    }

    // Inativa uma vaga (RN01: nunca deletar fisicamente)
    public async Task Inativar(int id)
    {
        var vaga = await _context.Vagas.FindAsync(id);

        if (vaga == null)
            throw new Exception("Vaga não encontrada.");

        // Verifica se a vaga tem reservas ativas (ocupada no momento)
        var ocupada = await _context.Reservas
            .AnyAsync(r => r.VagaId == id && r.Saida == null);

        if (ocupada)
            throw new Exception("Não é possível inativar uma vaga que está ocupada.");

        vaga.Ativa = false;
        await _context.SaveChangesAsync();
    }
}