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