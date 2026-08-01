using MesaSitec.Aplicacion.Solicitudes;
using MesaSitec.Dominio;
using MesaSitec.Dominio.Excepciones;
using Xunit;

namespace MesaSitec.Tests;

public class MaquinaEstadosTests
{
    [Fact]
    public void AplicarTransicion_NuevaAsignar_DevuelveAsignada()
    {
        var resultado = MaquinaEstados.AplicarTransicion(EstadoSolicitud.Nueva, AccionSolicitud.Asignar);
        Assert.Equal(EstadoSolicitud.Asignada, resultado);
    }

    [Fact]
    public void AplicarTransicion_NuevaResolver_LanzaTransicionInvalida()
    {
        Assert.Throws<TransicionInvalidaException>(() =>
            MaquinaEstados.AplicarTransicion(EstadoSolicitud.Nueva, AccionSolicitud.Resolver));
    }

    [Fact]
    public void AplicarTransicion_CerradaCualquierAccion_LanzaTransicionInvalida()
    {
        Assert.Throws<TransicionInvalidaException>(() =>
            MaquinaEstados.AplicarTransicion(EstadoSolicitud.Cerrada, AccionSolicitud.Reabrir));
    }

    [Fact]
    public void AplicarTransicion_ResueltaReabrir_DevuelveEnProceso()
    {
        var resultado = MaquinaEstados.AplicarTransicion(EstadoSolicitud.Resuelta, AccionSolicitud.Reabrir);
        Assert.Equal(EstadoSolicitud.EnProceso, resultado);
    }
}