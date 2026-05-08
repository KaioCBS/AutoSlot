using AutoSlot.Data;
using AutoSlot.DTOs;
using AutoSlot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoSlot.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // Registra um novo funcionário no banco
    public async Task<Funcionario> Registrar(string nome, string email, string senha, string nivelAcesso)
    {
        // Verifica se já existe um funcionário com esse email
        var existe = await _context.Funcionarios
            .AnyAsync(f => f.Email == email);

        if (existe)
            throw new Exception("Email já cadastrado.");

        var funcionario = new Funcionario
        {
            Nome = nome,
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha), 
            NivelAcesso = nivelAcesso,
            Ativo = true,
            CriadoEm = DateTime.UtcNow // ← mudou
        };

        _context.Funcionarios.Add(funcionario);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Mostra o erro completo incluindo o inner exception
            var mensagemCompleta = ex.InnerException?.Message ?? ex.Message;
            throw new Exception(mensagemCompleta);
        }

        return funcionario; 
    }

    // Faz login e retorna o token JWT
    public async Task<string?> Login(LoginDTO dto)
    {
        // Busca o funcionário pelo email
        var funcionario = await _context.Funcionarios
            .FirstOrDefaultAsync(f => f.Email == dto.Email && f.Ativo);

        if (funcionario == null)
            return null; // email não encontrado ou inativo

        // Verifica se a senha está correta
        bool senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Senha, funcionario.SenhaHash);
        if (!senhaCorreta)
            return null;

        // Gera o token JWT
        return GerarToken(funcionario);
    }

    private string GerarToken(Funcionario funcionario)
    {
        var jwtKey = _configuration["Jwt:Key"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims = informações que ficam dentro do token
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, funcionario.Id.ToString()),
            new Claim(ClaimTypes.Email, funcionario.Email),
            new Claim(ClaimTypes.Name, funcionario.Nome),
            new Claim(ClaimTypes.Role, funcionario.NivelAcesso) // "Admin" ou "Funcionario"
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddHours(8), // token expira em 8 horas
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}