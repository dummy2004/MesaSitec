using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio;
using Xunit;

namespace MesaSitec.Tests;

public class PermisosTests
{
    [Fact]
    public void PuedeEditar_SolicitanteConSolicitudPropiaEnNueva_DevuelveTrue()
    {
        var usuarioId = Guid.NewGuid();
        var solicitud = new Solicitud { SolicitanteId = usuarioId, Estado = EstadoSolicitud.Nueva };

        var resultado = PermisosSolicitud.PuedeEditar(RolUsuario.Solicitante, usuarioId, solicitud);

        Assert.True(resultado);
    }

    [Fact]
    public void PuedeEditar_SolicitanteConSolicitudPropiaEnProceso_DevuelveFalse()
    {
        var usuarioId = Guid.NewGuid();
        var solicitud = new Solicitud { SolicitanteId = usuarioId, Estado = EstadoSolicitud.EnProceso };

        var resultado = PermisosSolicitud.PuedeEditar(RolUsuario.Solicitante, usuarioId, solicitud);

        Assert.False(resultado);
    }

    [Fact]
    public void PuedeEjecutarAccionDeFlujo_CancelarComoAgente_DevuelveFalse()
    {
        var solicitud = new Solicitud { SolicitanteId = Guid.NewGuid(), Estado = EstadoSolicitud.Nueva };

        var resultado = PermisosSolicitud.PuedeEjecutarAccionDeFlujo(
            RolUsuario.Agente, AccionSolicitud.Cancelar, Guid.NewGuid(), solicitud);

        Assert.False(resultado);
    }
}