namespace MesaSitec.Dominio.Excepciones;

public abstract class DominioException : Exception
{
    public string Codigo { get; }

    protected DominioException(string codigo, string mensaje) : base(mensaje)
    {
        Codigo = codigo;
    }
}

public class TransicionInvalidaException : DominioException
{
    public TransicionInvalidaException(string mensaje)
        : base("TRANSICION_INVALIDA", mensaje) { }
}

public class OperacionNoPermitidaException : DominioException
{
    public OperacionNoPermitidaException(string mensaje)
        : base("OPERACION_NO_PERMITIDA", mensaje) { }
}

public class AgenteInvalidoException : DominioException
{
    public AgenteInvalidoException(string mensaje)
        : base("AGENTE_INVALIDO", mensaje) { }
}

public class MotivoRequeridoException : DominioException
{
    public MotivoRequeridoException(string mensaje)
        : base("MOTIVO_REQUERIDO", mensaje) { }
}

public class RecursoNoEncontradoException : DominioException
{
    public RecursoNoEncontradoException(string mensaje)
        : base("RECURSO_NO_ENCONTRADO", mensaje) { }
}

public class NoAutenticadoException : DominioException
{
    public NoAutenticadoException(string mensaje)
        : base("NO_AUTENTICADO", mensaje) { }
}

public class ParametroInvalidoException : DominioException
{
    public ParametroInvalidoException(string mensaje)
        : base("PARAMETRO_INVALIDO", mensaje) { }
}

public class ValidacionException : DominioException
{
    public Dictionary<string, string[]> Errores { get; }

    public ValidacionException(string campo, string mensaje)
        : base("VALIDACION", "Error de validación de campos.")
    {
        Errores = new Dictionary<string, string[]> { [campo] = new[] { mensaje } };
    }
}