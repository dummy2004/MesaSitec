using MesaSitec.Dominio;

namespace MesaSitec.Aplicacion.Auth;

public interface IJwtGenerator
{
    (string token, int expiraEnSegundos) Generar(Usuario usuario);
}