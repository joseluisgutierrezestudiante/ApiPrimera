using ApiPrimera.Models;

namespace ApiPrimera.Interfaces;

public interface IProductoRepository
{
    Task<IEnumerable<Producto>> GetAllAsync();
    Task<IEnumerable<Producto>> BuscarAsync(string? busqueda, string? marca, string? categoria, string? orden);
    Task<IEnumerable<string>> GetMarcasAsync();
    Task<IEnumerable<string>> GetCategoriasAsync();
    Task<Producto?> GetByIdAsync(int id);
    Task<Producto> CreateAsync(Producto producto);
    Task<bool> UpdateAsync(Producto producto);
    Task<bool> SetImagenUrlAsync(int id, string? imagenUrl);
    Task<bool> DeleteAsync(int id);
}