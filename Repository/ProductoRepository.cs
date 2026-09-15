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
        return await _context.Producto.ToListAsync();
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

    public async Task<bool> UpdateAsync(Producto producto)
    {
        var existing = await _context.Producto.FindAsync(producto.Id);
        if (existing == null) return false;

        existing.Nombre = producto.Nombre;
        existing.Descripcion = producto.Descripcion;
        existing.Precio = producto.Precio;
        existing.Stock = producto.Stock;

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
