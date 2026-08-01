using MesaSitec.Aplicacion.Sla;
using MesaSitec.Dominio;
using Xunit;

namespace MesaSitec.Tests;

public class SlaCalculatorTests
{
    private readonly ISlaCalculator _calculator = new SlaCalculator();

    [Fact]
    public void Calcular_IncidenteCritica_DevuelveCuatroHoras()
    {
        var fechaCreacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
        var resultado = _calculator.Calcular(fechaCreacion, slaHoras: 8, PrioridadSolicitud.Critica);
        Assert.Equal(fechaCreacion.AddHours(4), resultado);
    }

    [Fact]
    public void Calcular_ConsultaBaja_DevuelveCuarentaYOchoHoras()
    {
        var fechaCreacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
        var resultado = _calculator.Calcular(fechaCreacion, slaHoras: 24, PrioridadSolicitud.Baja);
        Assert.Equal(fechaCreacion.AddHours(48), resultado);
    }

    [Fact]
    public void Calcular_PrioridadMedia_NoAlteraElPlazoBase()
    {
        var fechaCreacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);
        var resultado = _calculator.Calcular(fechaCreacion, slaHoras: 40, PrioridadSolicitud.Media);
        Assert.Equal(fechaCreacion.AddHours(40), resultado);
    }
}