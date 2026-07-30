namespace MesaSitec.Aplicacion.Solicitudes;

public static class GeneradorCodigo
{
    public static string Generar(int correlativo, int anio)
        => $"SOL-{anio}-{correlativo:D5}";
}