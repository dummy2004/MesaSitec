using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MesaSitec.Api.Auth;
using MesaSitec.Aplicacion.Sla;
using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio;
using MesaSitec.Dominio.Excepciones;
using MesaSitec.Infraestructura;

namespace MesaSitec.Api.Controllers;

public record CrearSolicitudRequest(string Titulo, string Descripcion, Guid CategoriaId, string Prioridad);
public record EditarSolicitudRequest(string Titulo, string Descripcion, Guid CategoriaId, string Prioridad);
public record TransicionRequest(string Accion, Guid? AgenteId, string? Motivo);

[ApiController]
[Route("api/v1/solicitudes")]
[Authorize]
public class SolicitudesController(MesaSitecDbContext db, ISlaCalculator slaCalculator) : ControllerBase
{
    // ---- GET /solicitudes ----
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado, [FromQuery] string? prioridad, [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId, [FromQuery] string? q, [FromQuery] bool? vencidas,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string sort = "-fechaCreacion")
    {
        if (page < 1 || pageSize > 100)
            throw new ParametroInvalidoException("Los parámetros de paginación son inválidos.");

        var tenantId = User.TenantId();
        var rol = User.Rol();
        var usuarioId = User.UsuarioId();

        var query = db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Where(s => s.TenantId == tenantId);

        if (rol == RolUsuario.Solicitante.ToString())
            query = query.Where(s => s.SolicitanteId == usuarioId);

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(s => s.Estado == Enum.Parse<EstadoSolicitud>(estado));

        if (!string.IsNullOrEmpty(prioridad))
            query = query.Where(s => s.Prioridad == Enum.Parse<PrioridadSolicitud>(prioridad));

        if (categoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == categoriaId.Value);

        if (agenteId.HasValue)
            query = query.Where(s => s.AgenteId == agenteId.Value);

        if (!string.IsNullOrEmpty(q))
        {
            var qLower = q.ToLower();
            query = query.Where(s =>
                s.Titulo.ToLower().Contains(qLower) ||
                s.Descripcion.ToLower().Contains(qLower) ||
                s.Codigo.ToLower().Contains(qLower));
        }

        var ahora = DateTime.UtcNow;
        if (vencidas == true)
        {
            query = query.Where(s => s.FechaLimiteSla < ahora
                && s.Estado != EstadoSolicitud.Resuelta
                && s.Estado != EstadoSolicitud.Cerrada
                && s.Estado != EstadoSolicitud.Cancelada);
        }

        query = sort switch
        {
            "fechaCreacion" => query.OrderBy(s => s.FechaCreacion),
            "-fechaCreacion" => query.OrderByDescending(s => s.FechaCreacion),
            "codigo" => query.OrderBy(s => s.Codigo),
            "-codigo" => query.OrderByDescending(s => s.Codigo),
            "prioridad" => query.OrderBy(s => OrdenPrioridad(s.Prioridad)),
            "-prioridad" => query.OrderByDescending(s => OrdenPrioridad(s.Prioridad)),
            _ => query.OrderByDescending(s => s.FechaCreacion)
        };

        var total = await query.CountAsync();
        var totalPaginas = (int)Math.Ceiling(total / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                id = s.Id,
                codigo = s.Codigo,
                titulo = s.Titulo,
                estado = s.Estado.ToString(),
                prioridad = s.Prioridad.ToString(),
                categoria = new { id = s.Categoria.Id, nombre = s.Categoria.Nombre },
                agente = s.Agente == null ? null : new { id = s.Agente.Id, nombre = s.Agente.Nombre },
                fechaCreacion = s.FechaCreacion,
                fechaLimiteSla = s.FechaLimiteSla,
                vencida = s.FechaLimiteSla < ahora
                    && s.Estado != EstadoSolicitud.Resuelta
                    && s.Estado != EstadoSolicitud.Cerrada
                    && s.Estado != EstadoSolicitud.Cancelada
            })
            .ToListAsync();

        return Ok(new { items, page, pageSize, total, totalPaginas });
    }

    private static int OrdenPrioridad(PrioridadSolicitud p) => p switch
    {
        PrioridadSolicitud.Critica => 0,
        PrioridadSolicitud.Alta => 1,
        PrioridadSolicitud.Media => 2,
        PrioridadSolicitud.Baja => 3,
        _ => 4
    };

    // ---- POST /solicitudes ----
    [HttpPost]
    public async Task<IActionResult> Crear(CrearSolicitudRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo) || request.Titulo.Length < 5 || request.Titulo.Length > 120)
            throw new ValidacionException("titulo", "El título debe tener entre 5 y 120 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Descripcion) || request.Descripcion.Length < 10 || request.Descripcion.Length > 4000)
            throw new ValidacionException("descripcion", "La descripción debe tener entre 10 y 4000 caracteres.");

        if (!Enum.TryParse<PrioridadSolicitud>(request.Prioridad, out var prioridad))
            throw new ValidacionException("prioridad", "La prioridad enviada no es válida.");

        var tenantId = User.TenantId();
        var usuarioId = User.UsuarioId();

        var categoria = await db.Categorias
            .FirstOrDefaultAsync(c => c.Id == request.CategoriaId && c.TenantId == tenantId && c.Activo);

        if (categoria is null)
            throw new ValidacionException("categoriaId", "La categoría enviada no existe o no está activa.");

        var fechaCreacion = DateTime.UtcNow;
        var fechaLimite = slaCalculator.Calcular(fechaCreacion, categoria.SlaHoras, prioridad);

        var anioActual = fechaCreacion.Year;
        var correlativo = await db.Solicitudes
            .Where(s => s.TenantId == tenantId && s.FechaCreacion.Year == anioActual)
            .CountAsync() + 1;

        var solicitud = new Solicitud
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = GeneradorCodigo.Generar(correlativo, anioActual),
            Titulo = request.Titulo,
            Descripcion = request.Descripcion,
            CategoriaId = categoria.Id,
            Prioridad = prioridad,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = usuarioId,
            FechaCreacion = fechaCreacion,
            FechaLimiteSla = fechaLimite
        };

        db.Solicitudes.Add(solicitud);
        await db.SaveChangesAsync();

        return Created($"/api/v1/solicitudes/{solicitud.Id}", await ObtenerDetalleDto(solicitud.Id));
    }

    // ---- GET /solicitudes/{id} ----
    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(Guid id)
    {
        var tenantId = User.TenantId();
        var rol = User.Rol();
        var usuarioId = User.UsuarioId();

        var solicitud = await BuscarOFallar(id, tenantId);

        if (rol == RolUsuario.Solicitante.ToString() && solicitud.SolicitanteId != usuarioId)
            throw new RecursoNoEncontradoException("Solicitud no encontrada.");

        return Ok(await ObtenerDetalleDto(id));
    }

    // ---- PUT /solicitudes/{id} ----
    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(Guid id, EditarSolicitudRequest request)
    {
        var tenantId = User.TenantId();
        var rol = User.Rol();
        var usuarioId = User.UsuarioId();

        var solicitud = await BuscarOFallar(id, tenantId);

        var rolEnum = Enum.Parse<RolUsuario>(rol);
        var puedeEditar = PermisosSolicitud.PuedeEditar(rolEnum, usuarioId, solicitud);

        if (!puedeEditar)
            throw new OperacionNoPermitidaException("No tienes permiso para editar esta solicitud.");

        if (string.IsNullOrWhiteSpace(request.Titulo) || request.Titulo.Length < 5 || request.Titulo.Length > 120)
            throw new ValidacionException("titulo", "El título debe tener entre 5 y 120 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Descripcion) || request.Descripcion.Length < 10 || request.Descripcion.Length > 4000)
            throw new ValidacionException("descripcion", "La descripción debe tener entre 10 y 4000 caracteres.");

        if (!Enum.TryParse<PrioridadSolicitud>(request.Prioridad, out var prioridad))
            throw new ValidacionException("prioridad", "La prioridad enviada no es válida.");

        var categoria = await db.Categorias
            .FirstOrDefaultAsync(c => c.Id == request.CategoriaId && c.TenantId == tenantId && c.Activo);

        if (categoria is null)
            throw new ValidacionException("categoriaId", "La categoría enviada no existe o no está activa.");

        bool recalcularSla = categoria.Id != solicitud.CategoriaId || prioridad != solicitud.Prioridad;

        solicitud.Titulo = request.Titulo;
        solicitud.Descripcion = request.Descripcion;
        solicitud.CategoriaId = categoria.Id;
        solicitud.Prioridad = prioridad;

        if (recalcularSla)
            solicitud.FechaLimiteSla = slaCalculator.Calcular(solicitud.FechaCreacion, categoria.SlaHoras, prioridad);

        await db.SaveChangesAsync();

        return Ok(await ObtenerDetalleDto(id));
    }

    // ---- POST /solicitudes/{id}/transiciones ----
    [HttpPost("{id}/transiciones")]
    public async Task<IActionResult> Transicionar(Guid id, TransicionRequest request)
    {
        var tenantId = User.TenantId();
        var rol = User.Rol();
        var usuarioId = User.UsuarioId();

        var solicitud = await BuscarOFallar(id, tenantId);

        if (!Enum.TryParse<AccionSolicitud>(request.Accion, ignoreCase: true, out var accion))
            throw new ValidacionException("accion", "La acción enviada no es válida.");

        var rolEnum = Enum.Parse<RolUsuario>(rol);
        if (!PermisosSolicitud.PuedeEjecutarAccionDeFlujo(rolEnum, accion, usuarioId, solicitud))
            throw new OperacionNoPermitidaException("No tienes permiso para ejecutar esta acción.");

        var nuevoEstado = MaquinaEstados.AplicarTransicion(solicitud.Estado, accion);

        if (accion == AccionSolicitud.Asignar)
        {
            if (!request.AgenteId.HasValue)
                throw new AgenteInvalidoException("Debes especificar un agente para asignar.");

            var agente = await db.Usuarios.FirstOrDefaultAsync(u =>
                u.Id == request.AgenteId.Value && u.TenantId == tenantId);

            var agenteValido = agente is not null && agente.Activo
                && (agente.Rol == RolUsuario.Agente || agente.Rol == RolUsuario.Admin);

            if (!agenteValido)
                throw new AgenteInvalidoException("El agente especificado no es válido.");

            solicitud.AgenteId = agente!.Id;
        }

        if (accion == AccionSolicitud.Resolver)
        {
            if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Length < 20)
                throw new MotivoRequeridoException("El motivo de resolución debe tener al menos 20 caracteres.");

            solicitud.MotivoResolucion = request.Motivo;
            solicitud.FechaResolucion = DateTime.UtcNow;
        }

        if (accion == AccionSolicitud.Cancelar)
        {
            if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Length < 10)
                throw new MotivoRequeridoException("El motivo de cancelación debe tener al menos 10 caracteres.");

            solicitud.MotivoCancelacion = request.Motivo;
        }

        solicitud.Estado = nuevoEstado;
        await db.SaveChangesAsync();

        return Ok(await ObtenerDetalleDto(id));
    }

    // ---- Helpers privados ----
    private async Task<Solicitud> BuscarOFallar(Guid id, Guid tenantId)
    {
        var solicitud = await db.Solicitudes.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);
        return solicitud ?? throw new RecursoNoEncontradoException("Solicitud no encontrada.");
    }

    private async Task<object> ObtenerDetalleDto(Guid id)
    {
        var ahora = DateTime.UtcNow;

        return await db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Include(s => s.Solicitante)
            .Where(s => s.Id == id)
            .Select(s => new
            {
                id = s.Id,
                codigo = s.Codigo,
                titulo = s.Titulo,
                descripcion = s.Descripcion,
                estado = s.Estado.ToString(),
                prioridad = s.Prioridad.ToString(),
                categoria = new { id = s.Categoria.Id, nombre = s.Categoria.Nombre },
                agente = s.Agente == null ? null : new { id = s.Agente.Id, nombre = s.Agente.Nombre },
                solicitante = new { id = s.Solicitante.Id, nombre = s.Solicitante.Nombre },
                fechaCreacion = s.FechaCreacion,
                fechaLimiteSla = s.FechaLimiteSla,
                fechaResolucion = s.FechaResolucion,
                motivoResolucion = s.MotivoResolucion,
                motivoCancelacion = s.MotivoCancelacion,
                vencida = s.FechaLimiteSla < ahora
                    && s.Estado != EstadoSolicitud.Resuelta
                    && s.Estado != EstadoSolicitud.Cerrada
                    && s.Estado != EstadoSolicitud.Cancelada
            })
            .FirstAsync();
    }
}