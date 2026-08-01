using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MesaSitec.Aplicacion.Auth;
using MesaSitec.Api.Auth;
using MesaSitec.Dominio;
using MesaSitec.Dominio.Excepciones;
using MesaSitec.Infraestructura;

namespace MesaSitec.Api.Controllers;

public record LoginRequest(string Email, string Password);

[ApiController]
[Route("api/v1")]
public class AuthController(MesaSitecDbContext db, IJwtGenerator jwtGenerator) : ControllerBase
{
    [HttpPost("auth/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario is null || !usuario.Activo)
            throw new NoAutenticadoException("Email o contraseña incorrectos.");

        var hasher = new PasswordHasher<Usuario>();
        var resultado = hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);

        if (resultado == PasswordVerificationResult.Failed)
            throw new NoAutenticadoException("Email o contraseña incorrectos.");

        var (token, expiraEn) = jwtGenerator.Generar(usuario);

        return Ok(new
        {
            accessToken = token,
            expiraEn,
            usuario = MapearUsuario(usuario)
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var usuario = await db.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == User.UsuarioId());

        if (usuario is null)
            throw new RecursoNoEncontradoException("Usuario no encontrado.");

        return Ok(MapearUsuario(usuario));
    }

    private static object MapearUsuario(Usuario u) => new
    {
        id = u.Id,
        nombre = u.Nombre,
        email = u.Email,
        rol = u.Rol.ToString(),
        tenantId = u.TenantId,
        tenantNombre = u.Tenant.Nombre
    };
}