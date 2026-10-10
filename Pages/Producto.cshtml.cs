using ApiPrimera.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class ProductoModel : PageModel
{
    private readonly IProductoApiClient _api;

    public ProductoModel(IProductoApiClient api)
    {
        _api = api;
    }

    public ApiPrimera.Models.Producto? Producto { get; private set; }

    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var resultado = await _api.ObtenerPorIdAsync(id);

        if (resultado.NoEncontrado)
        {
            return NotFound();
        }

        if (!resultado.Exito || resultado.Valor is null)
        {
            Error = resultado.Error ?? "Ocurrió un error inesperado.";
            return Page();
        }

        Producto = resultado.Valor;
        return Page();
    }
}