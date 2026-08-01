using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MesaSitec.Api.Auth;
using MesaSitec.Infraestructura;

namespace MesaSitec.Api.Controllers;

[ApiController]
[Route("api/v1/categorias")]
[Authorize]
public class CategoriasController(MesaSitecDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var tenantId = User.TenantId();

        var categorias = await db.Categorias
            .Where(c => c.TenantId == tenantId && c.Activo)
            .Select(c => new { id = c.Id, nombre = c.Nombre, slaHoras = c.SlaHoras })
            .ToListAsync();

        return Ok(categorias);
    }
}