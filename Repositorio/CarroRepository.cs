using ApiPrimera.Models;
using ApiPrimera.Interfaces;

namespace ApiPrimera.Repositorio;

public class CarroRepository : ICarroRepository
{
    private readonly List<Carro> _carros = new();
    private int _nextId = 1;
    private readonly IMarcaRepository _marcaRepo;

    public CarroRepository(IMarcaRepository marcaRepo)
    {
        _marcaRepo = marcaRepo;

        // Datos iniciales de ejemplo
        var marca = _marcaRepo.GetAll().FirstOrDefault();
        if (marca != null)
        {
            Create(new Carro { Modelo = "Modelo A", Precio = 10000m, MarcaId = marca.Id });
        }
    }

    public Carro Create(Carro carro)
    {
        var marca = _marcaRepo.GetById(carro.MarcaId);
        if (marca == null)
            throw new ArgumentException("Marca no encontrada.");

        carro.Id = _nextId++;
        carro.Marca = marca;
        ApplyDiscount(carro, marca);
        _carros.Add(carro);
        return carro;
    }

    public bool Delete(int id)
    {
        var c = GetById(id);
        if (c == null) return false;
        return _carros.Remove(c);
    }

    public IEnumerable<Carro> GetAll()
    {
        // Asegurar que cada carro tenga su marca enlazada
        foreach (var c in _carros)
        {
            c.Marca = _marcaRepo.GetById(c.MarcaId);
            if (c.Marca != null) ApplyDiscount(c, c.Marca);
        }
        return _carros;
    }

    public Carro? GetById(int id)
    {
        var c = _carros.FirstOrDefault(x => x.Id == id);
        if (c != null)
        {
            c.Marca = _marcaRepo.GetById(c.MarcaId);
            if (c.Marca != null) ApplyDiscount(c, c.Marca);
        }
        return c;
    }

    public bool Update(Carro carro)
    {
        var existing = GetById(carro.Id);
        if (existing == null) return false;
        var marca = _marcaRepo.GetById(carro.MarcaId);
        if (marca == null) throw new ArgumentException("Marca no encontrada.");

        existing.Modelo = carro.Modelo;
        existing.Precio = carro.Precio;
        existing.MarcaId = carro.MarcaId;
        existing.Marca = marca;
        ApplyDiscount(existing, marca);
        return true;
    }

    private void ApplyDiscount(Carro carro, Marca marca)
    {
        var pct = marca.DiscountPercentage;
        if (pct <= 0)
        {
            carro.PrecioConDescuento = carro.Precio;
            return;
        }
        carro.PrecioConDescuento = Math.Round(carro.Precio * (1 - pct / 100m), 2);
    }
}
