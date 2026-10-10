using ApiPrimera.Models;

namespace ApiPrimera.Data;

public static class InMemoryData
{
    public static List<Marca> Marcas { get; } = new List<Marca>
    {
        new Marca { Id = 1, Nombre = "Toyota", Descripcion = "Marca Toyota", DiscountPercentage = 15m },
        new Marca { Id = 2, Nombre = "Renault", Descripcion = "Marca Renault", DiscountPercentage = 25m },
        new Marca { Id = 3, Nombre = "Chevrolet", Descripcion = "Marca Chevrolet", DiscountPercentage = 20m },
        new Marca { Id = 4, Nombre = "Genérico", Descripcion = "Marca genérica", DiscountPercentage = 0m }
    };

    public static List<Carro> Carros { get; } = new List<Carro>();
}
