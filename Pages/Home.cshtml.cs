using System.Globalization;
using ApiPrimera.Interfaces;
using ApiPrimera.Models;
using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class HomeModel : PageModel
{
    private readonly IProductoApiClient _api;

    public HomeModel(IProductoApiClient api)
    {
        _api = api;
    }

    public CatalogoViewModel Catalogo { get; private set; } = new();

    public async Task OnGetAsync(string? busqueda, string? marca, string? categoria, string? orden)
    {
        var productosTask = _api.ObtenerTodosAsync(busqueda, marca, categoria, orden);
        var marcasTask = _api.ObtenerMarcasAsync();
        var categoriasTask = _api.ObtenerCategoriasAsync();

        await Task.WhenAll(productosTask, marcasTask, categoriasTask);

        var productos = await productosTask;
        var marcas = await marcasTask;
        var categorias = await categoriasTask;

        Catalogo = new CatalogoViewModel
        {
            Productos = productos.Exito ? productos.Valor?.ToList() ?? new List<Producto>() : new List<Producto>(),
            Marcas = marcas.Exito ? marcas.Valor?.ToList() ?? new List<string>() : new List<string>(),
            Categorias = categorias.Exito ? categorias.Valor?.ToList() ?? new List<string>() : new List<string>(),
            Busqueda = busqueda,
            Marca = marca,
            Categoria = categoria,
            Orden = orden,
            Error = productos.Exito ? null : productos.Error
        };
    }

    public static string FormatoPrecio(decimal precio) =>
        precio.ToString("C0", CultureInfo.GetCultureInfo("es-CO"));

    public static string FormatoKilometraje(int kilometraje) =>
        kilometraje > 0 ? $"{kilometraje:N0} km".Replace(",", ".") : "Nuevo";
}