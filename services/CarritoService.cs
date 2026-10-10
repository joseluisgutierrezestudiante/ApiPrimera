using System.Text.Json;
using ApiPrimera.Interfaces;
using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace ApiPrimera.Services;

public class CarritoService : ICarritoService
{
    private const string ClaveSesion = "Carrito";

    private readonly ISession _sesion;
    private List<LineaCarrito>? _cache;

    public CarritoService(IHttpContextAccessor acceso)
    {
        _sesion = acceso.HttpContext?.Session
            ?? throw new InvalidOperationException("No hay sesion HTTP disponible para el carrito.");
    }

    public int Conteo => LineasInternas().Sum(l => l.Cantidad);

    public IReadOnlyList<LineaCarrito> Lineas => LineasInternas();

    public void Agregar(int productoId, int cantidad = 1)
    {
        if (cantidad <= 0)
        {
            return;
        }

        var lineas = LineasInternas();
        var linea = lineas.FirstOrDefault(l => l.ProductoId == productoId);

        if (linea is not null)
        {
            linea.Cantidad += cantidad;
        }
        else
        {
            lineas.Add(new LineaCarrito { ProductoId = productoId, Cantidad = cantidad });
        }

        Guardar();
    }

    public void ActualizarCantidad(int productoId, int cantidad)
    {
        var lineas = LineasInternas();
        var linea = lineas.FirstOrDefault(l => l.ProductoId == productoId);

        if (linea is null)
        {
            return;
        }

        if (cantidad <= 0)
        {
            lineas.Remove(linea);
        }
        else
        {
            linea.Cantidad = cantidad;
        }

        Guardar();
    }

    public void Quitar(int productoId)
    {
        var lineas = LineasInternas();
        lineas.RemoveAll(l => l.ProductoId == productoId);
        Guardar();
    }

    public void Vaciar()
    {
        _cache = new List<LineaCarrito>();
        _sesion.Remove(ClaveSesion);
    }

    private List<LineaCarrito> LineasInternas()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        var json = _sesion.GetString(ClaveSesion);
        _cache = string.IsNullOrEmpty(json)
            ? new List<LineaCarrito>()
            : JsonSerializer.Deserialize<List<LineaCarrito>>(json) ?? new List<LineaCarrito>();

        return _cache;
    }

    private void Guardar()
    {
        _sesion.SetString(ClaveSesion, JsonSerializer.Serialize(_cache ?? new List<LineaCarrito>()));
    }
}