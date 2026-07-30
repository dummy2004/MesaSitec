using MesaSitec.Dominio;
using MesaSitec.Dominio.Excepciones;

namespace MesaSitec.Aplicacion.Solicitudes;

public enum AccionSolicitud
{
    Asignar,
    Iniciar,
    Resolver,
    Cerrar,
    Reabrir,
    Cancelar
}

public static class MaquinaEstados
{
    private static readonly Dictionary<EstadoSolicitud, HashSet<AccionSolicitud>> Transiciones = new()
    {
        [EstadoSolicitud.Nueva] = new() { AccionSolicitud.Asignar, AccionSolicitud.Cancelar },
        [EstadoSolicitud.Asignada] = new() { AccionSolicitud.Iniciar, AccionSolicitud.Asignar, AccionSolicitud.Cancelar },
        [EstadoSolicitud.EnProceso] = new() { AccionSolicitud.Resolver, AccionSolicitud.Asignar, AccionSolicitud.Cancelar },
        [EstadoSolicitud.Resuelta] = new() { AccionSolicitud.Cerrar, AccionSolicitud.Reabrir },
        [EstadoSolicitud.Cerrada] = new(),
        [EstadoSolicitud.Cancelada] = new()
    };

    private static readonly Dictionary<AccionSolicitud, EstadoSolicitud> EstadoDestino = new()
    {
        [AccionSolicitud.Asignar] = EstadoSolicitud.Asignada,
        [AccionSolicitud.Iniciar] = EstadoSolicitud.EnProceso,
        [AccionSolicitud.Resolver] = EstadoSolicitud.Resuelta,
        [AccionSolicitud.Cerrar] = EstadoSolicitud.Cerrada,
        [AccionSolicitud.Reabrir] = EstadoSolicitud.EnProceso,
        [AccionSolicitud.Cancelar] = EstadoSolicitud.Cancelada
    };

    public static EstadoSolicitud AplicarTransicion(EstadoSolicitud estadoActual, AccionSolicitud accion)
    {
        if (!Transiciones[estadoActual].Contains(accion))
        {
            throw new TransicionInvalidaException(
                $"No se puede aplicar '{accion}' sobre una solicitud en estado '{estadoActual}'.");
        }

        return EstadoDestino[accion];
    }
}