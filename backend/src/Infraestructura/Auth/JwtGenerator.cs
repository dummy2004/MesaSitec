using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MesaSitec.Aplicacion.Auth;
using MesaSitec.Dominio;

namespace MesaSitec.Infraestructura.Auth;

public class JwtGenerator(IConfiguration config) : IJwtGenerator
{
    private const int ExpiracionHoras = 8;

    public (string token, int expiraEnSegundos) Generar(Usuario usuario)
    {
        var secreto = config["JWT_SECRET"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("JWT_SECRET no está configurado.");

        var claves = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secreto));
        var credenciales = new SigningCredentials(claves, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim("tenantId", usuario.TenantId.ToString()),
            new Claim("rol", usuario.Rol.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email)
        };

        var expiracion = DateTime.UtcNow.AddHours(ExpiracionHoras);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiracion,
            signingCredentials: credenciales
        );

        var tokenSerializado = new JwtSecurityTokenHandler().WriteToken(token);
        var segundos = ExpiracionHoras * 3600;

        return (tokenSerializado, segundos);
    }
}