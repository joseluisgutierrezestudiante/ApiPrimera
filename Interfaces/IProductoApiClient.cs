using ApiPrimera.Models;

namespace ApiPrimera.Interfaces;

public interface IProductoApiClient
{
    Task<ResultadoApi<IEnumerable<Producto>>> ObtenerTodosAsync(
        string? busqueda = null,
        string? marca = null,
        string? categoria = null,
        string? orden = null);

    Task<ResultadoApi<IEnumerable<string>>> ObtenerMarcasAsync();

    Task<ResultadoApi<IEnumerable<string>>> ObtenerCategoriasAsync();

    Task<ResultadoApi<Producto>> ObtenerPorIdAsync(int id);

    Task<ResultadoApi<Producto>> CrearAsync(Producto producto);

    Task<ResultadoApi<Producto>> ActualizarAsync(Producto producto);

    Task<ResultadoApi<bool>> EliminarAsync(int id);

    Task<ResultadoApi<Producto>> SubirImagenAsync(int id, Stream contenido, string nombreArchivo, string contentType);

    Task<ResultadoApi<bool>> EliminarImagenAsync(int id);
}