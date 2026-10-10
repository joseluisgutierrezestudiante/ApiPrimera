namespace ApiPrimera.Models;

public sealed record ResultadoApi<T>(bool Exito, T? Valor, bool NoEncontrado, string? Error)
{
    public static ResultadoApi<T> Ok(T valor) => new(true, valor, false, null);

    public static ResultadoApi<T> NotFound() => new(false, default, true, null);

    public static ResultadoApi<T> Fail(string error) => new(false, default, false, error);
}