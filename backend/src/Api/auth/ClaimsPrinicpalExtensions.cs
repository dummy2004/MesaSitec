using System.Security.Claims;

namespace MesaSitec.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid UsuarioId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub")
            ?? throw new InvalidOperationException("Token sin claim 'sub'."));

    public static Guid TenantId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue("tenantId")
            ?? throw new InvalidOperationException("Token sin claim 'tenantId'."));

    public static string Rol(this ClaimsPrincipal user)
        => user.FindFirstValue("rol")
            ?? throw new InvalidOperationException("Token sin claim 'rol'.");
}