using System.Text.Json;
using MesaSitec.Dominio.Excepciones;

namespace MesaSitec.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DominioException ex)
        {
            await EscribirError(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado");
            await EscribirErrorGenerico(context, 500, "ERROR_INTERNO", "Ocurrió un error inesperado.");
        }
    }

    private static int MapearStatus(DominioException ex) => ex switch
    {
        RecursoNoEncontradoException => 404,
        TransicionInvalidaException => 409,
        OperacionNoPermitidaException => 403,
        AgenteInvalidoException => 422,
        MotivoRequeridoException => 422,
        NoAutenticadoException => 401,
        ParametroInvalidoException => 400,
        ValidacionException => 422,
        _ => 400
    };

    private static async Task EscribirError(HttpContext context, DominioException ex)
    {
        var status = MapearStatus(ex);
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        object body = ex is ValidacionException ve
            ? new
            {
                type = $"https://mesasitec.local/errores/{ex.Codigo.ToLowerInvariant().Replace('_', '-')}",
                title = TituloPara(status),
                status,
                detail = ex.Message,
                codigo = ex.Codigo,
                errores = ve.Errores
            }
            : new
            {
                type = $"https://mesasitec.local/errores/{ex.Codigo.ToLowerInvariant().Replace('_', '-')}",
                title = TituloPara(status),
                status,
                detail = ex.Message,
                codigo = ex.Codigo
            };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }

    private static async Task EscribirErrorGenerico(HttpContext context, int status, string codigo, string detalle)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        var body = new
        {
            type = $"https://mesasitec.local/errores/{codigo.ToLowerInvariant().Replace('_', '-')}",
            title = TituloPara(status),
            status,
            detail = detalle,
            codigo
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }

    private static string TituloPara(int status) => status switch
    {
        400 => "Solicitud inválida",
        401 => "No autenticado",
        403 => "Operación no permitida",
        404 => "Recurso no encontrado",
        409 => "Conflicto de estado",
        422 => "Error de validación",
        _ => "Error interno"
    };
}
