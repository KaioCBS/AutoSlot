using AutoSlot.Domain.Models;
using AutoSlot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoSlot.Application.Services;

public class VagasService
{
    private readonly AppDbContext _context;

    public VagasService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vaga>> ListarTodas()
    {
        return await _context.Vagas
            .Where(v => v.Ativa)
            .ToListAsync();
    }

    public async Task<Vaga?> BuscarPorId(int id)
    {
        return await _context.Vagas
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vaga> Criar(string codigo)
    {
        var existe = await _context.Vagas.AnyAsync(v => v.Codigo == codigo);

        if (existe)
            throw new Exception($"Já existe uma vaga com o código '{codigo}'.");

        var vaga = new Vaga { Codigo = codigo, Ativa = true };
        _context.Vagas.Add(vaga);
        await _context.SaveChangesAsync();
        return vaga;
    }

    public async Task<Vaga> Editar(int id, string codigo)
    {
        var vaga = await _context.Vagas.FindAsync(id);

        if (vaga == null)
            throw new Exception("Vaga não encontrada.");

        var codigoEmUso = await _context.Vagas
            .AnyAsync(v => v.Codigo == codigo && v.Id != id);

        if (codigoEmUso)
            throw new Exception($"Já existe uma vaga com o código '{codigo}'.");

        vaga.Codigo = codigo;
        await _context.SaveChangesAsync();
        return vaga;
    }

    public async Task Inativar(int id)
    {
        var vaga = await _context.Vagas.FindAsync(id);

        if (vaga == null)
            throw new Exception("Vaga não encontrada.");

        var ocupada = await _context.Reservas
            .AnyAsync(r => r.VagaId == id && r.Saida == null);

        if (ocupada)
            throw new Exception("Não é possível inativar uma vaga que está ocupada.");

        vaga.Ativa = false;
        await _context.SaveChangesAsync();
    }
}