using ApiPrimera.DB;
using ApiPrimera.Interfaces;
using ApiPrimera.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiPrimera.Repository;

public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _context;

    public ProductoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        return await _context.Producto
            .OrderByDescending(p => p.Destacado)
            .ThenBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Producto>> BuscarAsync(string? busqueda, string? marca, string? categoria, string? orden)
    {
        var query = _context.Producto.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var termino = busqueda.Trim();
            query = query.Where(p =>
                p.Nombre.Contains(termino) ||
                p.Descripcion.Contains(termino) ||
                p.Marca.Contains(termino) ||
                p.Categoria.Contains(termino));
        }

        if (!string.IsNullOrWhiteSpace(marca))
        {
            var marcaNormalizada = marca.Trim();
            query = query.Where(p => p.Marca == marcaNormalizada);
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var categoriaNormalizada = categoria.Trim();
            query = query.Where(p => p.Categoria == categoriaNormalizada);
        }

        query = (orden?.Trim().ToLowerInvariant()) switch
        {
            "precio-asc" => query.OrderBy(p => p.Precio),
            "precio-desc" => query.OrderByDescending(p => p.Precio),
            "anio-desc" => query.OrderByDescending(p => p.Anio),
            "nombre-asc" => query.OrderBy(p => p.Nombre),
            _ => query.OrderByDescending(p => p.Destacado).ThenBy(p => p.Nombre)
        };

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<string>> GetMarcasAsync()
    {
        return await _context.Producto
            .Select(p => p.Marca)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetCategoriasAsync()
    {
        return await _context.Producto
            .Select(p => p.Categoria)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<Producto?> GetByIdAsync(int id)
    {
        return await _context.Producto.FindAsync(id);
    }

    public async Task<Producto> CreateAsync(Producto producto)
    {
        await _context.Producto.AddAsync(producto);
        await _context.SaveChangesAsync();
        return producto;
    }

    // ImagenUrl queda a cargo del endpoint de imagen (SetImagenUrlAsync) para que
    // ningun PUT pueda dejar un asset huerfano en Cloudinary sin eliminar.
    public async Task<bool> UpdateAsync(Producto producto)
    {
        var existing = await _context.Producto.FindAsync(producto.Id);
        if (existing == null) return false;

        existing.Nombre = producto.Nombre;
        existing.Descripcion = producto.Descripcion;
        existing.Marca = producto.Marca;
        existing.Categoria = producto.Categoria;
        existing.Anio = producto.Anio;
        existing.Kilometraje = producto.Kilometraje;
        existing.Combustible = producto.Combustible;
        existing.Transmision = producto.Transmision;
        existing.Color = producto.Color;
        existing.Precio = producto.Precio;
        existing.PrecioOriginal = producto.PrecioOriginal;
        existing.Stock = producto.Stock;
        existing.Destacado = producto.Destacado;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetImagenUrlAsync(int id, string? imagenUrl)
    {
        var existing = await _context.Producto.FindAsync(id);
        if (existing == null) return false;

        existing.ImagenUrl = imagenUrl;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Producto.FindAsync(id);
        if (existing == null) return false;
        _context.Producto.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}