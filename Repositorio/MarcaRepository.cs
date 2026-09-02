using ApiPrimera.Models;
using ApiPrimera.Interfaces;

namespace ApiPrimera.Repositorio;

public class MarcaRepository : IMarcaRepository
{
    private readonly List<Marca> _marcas = new();
    private int _nextId = 1;

    public MarcaRepository()
    {
        // Datos iniciales de ejemplo (agregar Descripcion)
        Create(new Marca { Nombre = "Genérico", Descripcion = "Marca genérica", DiscountPercentage = 0 });
        Create(new Marca { Nombre = "Económica", Descripcion = "Marca con precios bajos", DiscountPercentage = 5 });
        Create(new Marca { Nombre = "Premium", Descripcion = "Marca de alta gama", DiscountPercentage = 10 });
    }

    public Marca Create(Marca marca)
    {
        marca.Id = _nextId++;
        _marcas.Add(marca);
        return marca;
    }

    public bool Delete(int id)
    {
        var m = GetById(id);
        if (m == null) return false;
        return _marcas.Remove(m);
    }

    public IEnumerable<Marca> GetAll() => _marcas;

    public Marca? GetById(int id) => _marcas.FirstOrDefault(x => x.Id == id);

    public bool Update(Marca marca)
    {
        var existing = GetById(marca.Id);
        if (existing == null) return false;
        existing.Nombre = marca.Nombre;
        existing.DiscountPercentage = marca.DiscountPercentage;
        return true;
    }
}
