using ApiPrimera.Interfaces;
using ApiPrimera.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages.Admin;

[Microsoft.AspNetCore.Authorization.Authorize]

public class ProductosModel : PageModel
{
    private readonly IProductoApiClient _api;

    public ProductosModel(IProductoApiClient api)
    {
        _api = api;
    }

    public IReadOnlyList<Producto> Productos { get; private set; } = Array.Empty<Producto>();

    public string? Busqueda { get; private set; }

    public string? Marca { get; private set; }

    public string? Categoria { get; private set; }

    public string? Error { get; private set; }

    [TempData]
    public string? Mensaje { get; set; }

    public async Task OnGetAsync(string? busqueda, string? marca, string? categoria)
    {
        Busqueda = busqueda;
        Marca = marca;
        Categoria = categoria;

        var resultado = await _api.ObtenerTodosAsync(busqueda, marca, categoria, "nombre-asc");

        if (!resultado.Exito)
        {
            Error = resultado.Error ?? "No se pudo cargar el catálogo.";
            return;
        }

        Productos = resultado.Valor?.ToList() ?? new List<Producto>();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        var resultado = await _api.EliminarAsync(id);

        if (!resultado.Exito)
        {
            TempData["Mensaje"] = resultado.NoEncontrado
                ? "El producto ya no existe."
                : resultado.Error ?? "No se pudo eliminar el producto.";
        }
        else
        {
            TempData["Mensaje"] = "Producto eliminado.";
        }

        return RedirectToPage();
    }
}