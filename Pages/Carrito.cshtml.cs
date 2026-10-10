using ApiPrimera.Interfaces;
using ApiPrimera.Models;
using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class CarritoModel : PageModel
{
    private readonly ICarritoService _carrito;
    private readonly IProductoApiClient _api;

    public CarritoModel(ICarritoService carrito, IProductoApiClient api)
    {
        _carrito = carrito;
        _api = api;
    }

    public List<CarritoItemViewModel> Items { get; private set; } = new();

    public int Conteo { get; private set; }

    public decimal Total { get; private set; }

    public bool HayErrores { get; private set; }

    [TempData]
    public string? MensajeCompra { get; set; }

    public async Task OnGetAsync()
    {
        var error = await CargarAsync();
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }

    public async Task<IActionResult> OnPostAgregarAsync(int id, int cantidad = 1)
    {
        _carrito.Agregar(id, cantidad <= 0 ? 1 : cantidad);
        return RedirectToPage("/Carrito");
    }

    public IActionResult OnPostActualizarAsync(int productoId, int cantidad)
    {
        _carrito.ActualizarCantidad(productoId, cantidad);
        return RedirectToPage();
    }

    public IActionResult OnPostQuitarAsync(int productoId)
    {
        _carrito.Quitar(productoId);
        return RedirectToPage();
    }

    public IActionResult OnPostVaciarAsync()
    {
        _carrito.Vaciar();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostComprarAsync()
    {
        var lineas = _carrito.Lineas;
        if (lineas.Count == 0)
        {
            return RedirectToPage();
        }

        var lineasCompradas = 0;
        var errores = new List<string>();

        foreach (var linea in lineas.ToList())
        {
            var resultado = await _api.ObtenerPorIdAsync(linea.ProductoId);

            if (!resultado.Exito || resultado.Valor is null)
            {
                errores.Add($"No se pudo verificar el producto #{linea.ProductoId}.");
                continue;
            }

            var producto = resultado.Valor;

            if (producto.Stock < linea.Cantidad)
            {
                errores.Add($"'{producto.Nombre}' solo tiene {producto.Stock} unidad(es) disponible(s).");
                _carrito.ActualizarCantidad(producto.Id, Math.Max(producto.Stock, 0));
                continue;
            }

            producto.Stock -= linea.Cantidad;
            var actualizado = await _api.ActualizarAsync(producto);

            if (!actualizado.Exito)
            {
                errores.Add($"No se pudo completar la compra de '{producto.Nombre}'.");
                continue;
            }

            _carrito.Quitar(producto.Id);
            lineasCompradas++;
        }

        if (errores.Count > 0)
        {
            ModelState.AddModelError(string.Empty, string.Join(" ", errores));
            await CargarAsync();
            return Page();
        }

        MensajeCompra = lineasCompradas > 0
            ? $"Compra realizada. {lineasCompradas} vehiculo(s) reservado(s). Gracias por tu compra."
            : "No se pudo completar la compra.";

        return RedirectToPage();
    }

    private async Task<string?> CargarAsync()
    {
        var lineas = _carrito.Lineas;
        Conteo = _carrito.Conteo;

        var items = new List<CarritoItemViewModel>(lineas.Count);
        string? error = null;

        foreach (var linea in lineas)
        {
            var resultado = await _api.ObtenerPorIdAsync(linea.ProductoId);

            if (!resultado.Exito || resultado.Valor is null)
            {
                error = resultado.Error ?? "No se pudo cargar un producto del carrito.";
                continue;
            }

            items.Add(new CarritoItemViewModel
            {
                Producto = resultado.Valor,
                Cantidad = linea.Cantidad
            });
        }

        Items = items;
        Total = items.Sum(i => i.Subtotal);
        HayErrores = items.Any(i => i.SinStock);

        return error;
    }
}