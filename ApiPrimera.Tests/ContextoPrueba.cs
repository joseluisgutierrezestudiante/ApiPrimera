using ApiPrimera.DB;
using ApiPrimera.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ApiPrimera.Tests;

/// <summary>
/// Contexto de pruebas sobre SQLite en memoria: permite ejercitar el repositorio
/// sin depender de una instancia de MySQL.
/// </summary>
public class ContextoPrueba : IDisposable
{
    private readonly SqliteConnection _conexion;

    public ContextoPrueba()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();

        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conexion)
            .Options;

        Contexto = new AppDbContext(opciones);
        Contexto.Database.EnsureCreated();
    }

    public AppDbContext Contexto { get; }

    public Producto CrearProducto(
        string nombre = "Toyota Corolla",
        string marca = "Toyota",
        string categoria = "Sedan",
        decimal precio = 50_000_000m,
        int stock = 3,
        bool destacado = false,
        int anio = 2023,
        string? descripcion = "Sedan compacto")
    {
        var producto = new Producto
        {
            Nombre = nombre,
            Descripcion = descripcion ?? string.Empty,
            Marca = marca,
            Categoria = categoria,
            Anio = anio,
            Kilometraje = 15_000,
            Combustible = "Gasolina",
            Transmision = "Automática",
            Color = "Blanco",
            Precio = precio,
            PrecioOriginal = null,
            Stock = stock,
            Destacado = destacado
        };

        Contexto.Producto.Add(producto);
        Contexto.SaveChanges();

        return producto;
    }

    public void Dispose()
    {
        Contexto.Dispose();
        _conexion.Dispose();
        GC.SuppressFinalize(this);
    }
}