using MesaSitec.Dominio;
using Microsoft.AspNetCore.Identity;

namespace MesaSitec.Infraestructura;

public static class DbSeeder
{
    private const string PasswordSemilla = "Sitec.2026";

    public static void Sembrar(MesaSitecDbContext db, DateTime fechaBase)
    {
        if (db.Tenants.Any()) return;

        var hasher = new PasswordHasher<Usuario>();

        // ---- Tenants ----
        var norte = new Tenant { Id = Guid.NewGuid(), Nombre = "Cooperativa Norte", Activo = true };
        var sur = new Tenant { Id = Guid.NewGuid(), Nombre = "Bufete Sur", Activo = true };
        db.Tenants.AddRange(norte, sur);

        // ---- Usuarios ----
        Usuario CrearUsuario(Tenant tenant, string email, string nombre, RolUsuario rol)
        {
            var u = new Usuario
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                Email = email,
                Nombre = nombre,
                Rol = rol,
                Activo = true
            };
            u.PasswordHash = hasher.HashPassword(u, PasswordSemilla);
            return u;
        }

        var adminNorte = CrearUsuario(norte, "admin@norte.test", "Admin Norte", RolUsuario.Admin);
        var agente1Norte = CrearUsuario(norte, "agente1@norte.test", "Agente Uno Norte", RolUsuario.Agente);
        var agente2Norte = CrearUsuario(norte, "agente2@norte.test", "Agente Dos Norte", RolUsuario.Agente);
        var user1Norte = CrearUsuario(norte, "user1@norte.test", "Usuario Uno Norte", RolUsuario.Solicitante);
        var user2Norte = CrearUsuario(norte, "user2@norte.test", "Usuario Dos Norte", RolUsuario.Solicitante);

        var adminSur = CrearUsuario(sur, "admin@sur.test", "Admin Sur", RolUsuario.Admin);
        var user1Sur = CrearUsuario(sur, "user1@sur.test", "Usuario Uno Sur", RolUsuario.Solicitante);

        db.Usuarios.AddRange(adminNorte, agente1Norte, agente2Norte, user1Norte, user2Norte, adminSur, user1Sur);

        // ---- Categorías (en ambos tenants) ----
        Categoria CrearCategoria(Tenant tenant, string nombre, int slaHoras)
            => new() { Id = Guid.NewGuid(), TenantId = tenant.Id, Nombre = nombre, SlaHoras = slaHoras, Activo = true };

        var catIncidenteNorte = CrearCategoria(norte, "Incidente", 8);
        var catRequerimientoNorte = CrearCategoria(norte, "Requerimiento", 40);
        var catConsultaNorte = CrearCategoria(norte, "Consulta", 24);
        var catFallaNorte = CrearCategoria(norte, "Falla crítica", 4);

        var catIncidenteSur = CrearCategoria(sur, "Incidente", 8);
        var catRequerimientoSur = CrearCategoria(sur, "Requerimiento", 40);
        var catConsultaSur = CrearCategoria(sur, "Consulta", 24);
        var catFallaSur = CrearCategoria(sur, "Falla crítica", 4);

        db.Categorias.AddRange(
            catIncidenteNorte, catRequerimientoNorte, catConsultaNorte, catFallaNorte,
            catIncidenteSur, catRequerimientoSur, catConsultaSur, catFallaSur);

        // ---- Solicitudes ----
        var factor = new Dictionary<PrioridadSolicitud, double>
        {
            [PrioridadSolicitud.Critica] = 0.5,
            [PrioridadSolicitud.Alta] = 0.75,
            [PrioridadSolicitud.Media] = 1.0,
            [PrioridadSolicitud.Baja] = 2.0
        };

        var estados = new[]
        {
            EstadoSolicitud.Nueva, EstadoSolicitud.Asignada, EstadoSolicitud.EnProceso,
            EstadoSolicitud.Resuelta, EstadoSolicitud.Cerrada, EstadoSolicitud.Cancelada
        };
        var prioridades = new[]
        {
            PrioridadSolicitud.Baja, PrioridadSolicitud.Media, PrioridadSolicitud.Alta, PrioridadSolicitud.Critica
        };

        var solicitudesNorte = new List<Solicitud>();
        var categoriasNorte = new[] { catIncidenteNorte, catRequerimientoNorte, catConsultaNorte, catFallaNorte };
        var solicitantesNorte = new[] { user1Norte, user2Norte };
        var agentesNorte = new[] { agente1Norte, agente2Norte };

        for (int i = 1; i <= 25; i++)
        {
            var estado = estados[i % estados.Length];
            var prioridad = prioridades[i % prioridades.Length];
            var categoria = categoriasNorte[i % categoriasNorte.Length];
            var solicitante = solicitantesNorte[i % solicitantesNorte.Length];

            // Las primeras 5 (i=1..5) se fuerzan a Nueva y con fecha vieja -> vencidas
            bool forzarVencida = i <= 5;
            bool forzarResuelta = i > 5 && i <= 8;

            if (forzarVencida) estado = EstadoSolicitud.Nueva;
            if (forzarResuelta) estado = EstadoSolicitud.Resuelta;

            var fechaCreacion = forzarVencida
                ? fechaBase.AddDays(-30 - i)   // bien vieja, para asegurar que venció
                : fechaBase.AddDays(-i);

            var fechaLimite = fechaCreacion.AddHours(categoria.SlaHoras * factor[prioridad]);

            var sol = new Solicitud
            {
                Id = Guid.NewGuid(),
                TenantId = norte.Id,
                Codigo = $"SOL-{fechaBase.Year}-{i:D5}",
                Titulo = $"Solicitud de prueba número {i} - Cooperativa Norte",
                Descripcion = $"Descripción detallada de la solicitud de prueba numero {i} generada por el seeder.",
                CategoriaId = categoria.Id,
                Prioridad = prioridad,
                Estado = estado,
                SolicitanteId = solicitante.Id,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = fechaLimite
            };

            if (estado is EstadoSolicitud.Asignada or EstadoSolicitud.EnProceso or EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada)
            {
                sol.AgenteId = agentesNorte[i % agentesNorte.Length].Id;
            }

            if (estado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada)
            {
                sol.FechaResolucion = fechaCreacion.AddHours(1);
                sol.MotivoResolucion = "Se resolvió el inconveniente reportado y se validó con el usuario.";
            }

            if (estado == EstadoSolicitud.Cancelada)
            {
                sol.MotivoCancelacion = "Solicitud duplicada, se cierra a favor de otra existente.";
            }

            solicitudesNorte.Add(sol);
        }

        var solicitudesSur = new List<Solicitud>();
        var categoriasSur = new[] { catIncidenteSur, catRequerimientoSur, catConsultaSur, catFallaSur };

        for (int i = 1; i <= 8; i++)
        {
            var estado = estados[i % estados.Length];
            var prioridad = prioridades[i % prioridades.Length];
            var categoria = categoriasSur[i % categoriasSur.Length];

            var fechaCreacion = fechaBase.AddDays(-i);
            var fechaLimite = fechaCreacion.AddHours(categoria.SlaHoras * factor[prioridad]);

            var sol = new Solicitud
            {
                Id = Guid.NewGuid(),
                TenantId = sur.Id,
                Codigo = $"SOL-{fechaBase.Year}-{i:D5}",
                Titulo = $"Solicitud de prueba número {i} - Bufete Sur",
                Descripcion = $"Descripción detallada de la solicitud de prueba numero {i} generada por el seeder.",
                CategoriaId = categoria.Id,
                Prioridad = prioridad,
                Estado = estado,
                SolicitanteId = user1Sur.Id,
                FechaCreacion = fechaCreacion,
                FechaLimiteSla = fechaLimite
            };

            solicitudesSur.Add(sol);
        }

        db.Solicitudes.AddRange(solicitudesNorte);
        db.Solicitudes.AddRange(solicitudesSur);

        db.SaveChanges();
    }
}