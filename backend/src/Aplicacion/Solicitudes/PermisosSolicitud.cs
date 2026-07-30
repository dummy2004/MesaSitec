using MesaSitec.Dominio;

namespace MesaSitec.Aplicacion.Solicitudes;

public static class PermisosSolicitud
{
    public static bool PuedeListarTodas(RolUsuario rol)
        => rol is RolUsuario.Admin or RolUsuario.Agente;

    public static bool PuedeVerDetalle(RolUsuario rol, Guid usuarioId, Solicitud solicitud)
        => rol is RolUsuario.Admin or RolUsuario.Agente || solicitud.SolicitanteId == usuarioId;

    public static bool PuedeEditar(RolUsuario rol, Guid usuarioId, Solicitud solicitud)
    {
        if (rol is RolUsuario.Admin or RolUsuario.Agente) return true;
        return solicitud.SolicitanteId == usuarioId && solicitud.Estado == EstadoSolicitud.Nueva;
    }

    public static bool PuedeEjecutarAccionDeFlujo(RolUsuario rol, AccionSolicitud accion, Guid usuarioId, Solicitud solicitud)
    {
        if (accion == AccionSolicitud.Cancelar)
            return rol == RolUsuario.Admin;

        if (accion == AccionSolicitud.Cerrar)
            return rol is RolUsuario.Admin or RolUsuario.Agente
                   || (rol == RolUsuario.Solicitante && solicitud.SolicitanteId == usuarioId);

        // asignar / iniciar / resolver / reabrir
        return rol is RolUsuario.Admin or RolUsuario.Agente;
    }
}