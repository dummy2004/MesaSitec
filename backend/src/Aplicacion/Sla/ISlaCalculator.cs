using MesaSitec.Dominio;

namespace MesaSitec.Aplicacion.Sla;

public interface ISlaCalculator
{
    DateTime Calcular(DateTime fechaCreacion, int slaHoras, PrioridadSolicitud prioridad);
}

public class SlaCalculator : ISlaCalculator
{
    private static readonly Dictionary<PrioridadSolicitud, double> Factor = new()
    {
        [PrioridadSolicitud.Critica] = 0.5,
        [PrioridadSolicitud.Alta] = 0.75,
        [PrioridadSolicitud.Media] = 1.0,
        [PrioridadSolicitud.Baja] = 2.0
    };

    public DateTime Calcular(DateTime fechaCreacion, int slaHoras, PrioridadSolicitud prioridad)
        => fechaCreacion.AddHours(slaHoras * Factor[prioridad]);
}