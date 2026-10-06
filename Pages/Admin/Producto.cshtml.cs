using ApiPrimera.Interfaces;
using ApiPrimera.Models;
using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages.Admin;

public class ProductoModel : PageModel
{
    private readonly IProductoApiClient _api;

    public ProductoModel(IProductoApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public ProductoFormViewModel Input { get; set; } = new();

    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await CargarListasAsync();

        if (id is null or 0)
        {
            return Page();
        }

        var resultado = await _api.ObtenerPorIdAsync(id.Value);

        if (resultado.NoEncontrado)
        {
            return NotFound();
        }

        if (!resultado.Exito || resultado.Valor is null)
        {
            Error = resultado.Error ?? "No se pudo cargar el producto.";
            return Page();
        }

        Input = ProductoFormViewModel.FromProducto(resultado.Valor);
        Input.MarcasConocidas = (await ObtenerMarcasAsync()) ?? Array.Empty<string>();
        Input.CategoriasConocidas = (await ObtenerCategoriasAsync()) ?? Array.Empty<string>();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        await CargarListasAsync();

        var esNuevo = id is null or 0;
        Input.Id = esNuevo ? 0 : id!.Value;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var errorImagen = ImagenValidador.ValidarArchivo(Input.Imagen);
        if (errorImagen is not null)
        {
            ModelState.AddModelError("Input.Imagen", errorImagen);
            return Page();
        }

        var producto = Input.ToProducto();
        var resultado = esNuevo
            ? await _api.CrearAsync(producto)
            : await _api.ActualizarAsync(producto);

        if (!resultado.Exito || resultado.Valor is null)
        {
            Error = resultado.NoEncontrado
                ? "El producto ya no existe."
                : resultado.Error ?? "No se pudo guardar el producto.";
            return Page();
        }

        var idGuardado = resultado.Valor.Id;

        var errorImagenRemota = await AplicarImagenAsync(idGuardado);
        if (errorImagenRemota is not null)
        {
            Error = errorImagenRemota;
            Input.Id = idGuardado;
            return Page();
        }

        return RedirectToPage("/Admin/Producto", new { id = idGuardado, guardado = true });
    }

    public async Task<IActionResult> OnPostEliminarImagenAsync(int id)
    {
        var resultado = await _api.EliminarImagenAsync(id);

        if (!resultado.Exito)
        {
            TempData["Error"] = resultado.NoEncontrado
                ? "El producto ya no existe."
                : resultado.Error ?? "No se pudo eliminar la imagen.";
        }

        return RedirectToPage("/Admin/Producto", new { id });
    }

    private async Task<string?> AplicarImagenAsync(int id)
    {
        if (Input.QuitarImagen)
        {
            var borrado = await _api.EliminarImagenAsync(id);
            if (!borrado.Exito && !borrado.NoEncontrado)
            {
                return borrado.Error ?? "No se pudo eliminar la imagen anterior.";
            }

            return null;
        }

        if (Input.Imagen is null || Input.Imagen.Length == 0)
        {
            return null;
        }

        await using var stream = Input.Imagen.OpenReadStream();
        var subida = await _api.SubirImagenAsync(
            id,
            stream,
            Input.Imagen.FileName,
            ImagenValidador.ContentTypeSeguro(Input.Imagen));

        return subida.Exito
            ? null
            : subida.Error ?? "El producto se guardó, pero la imagen no se pudo subir.";
    }

    private async Task CargarListasAsync()
    {
        Input.MarcasConocidas = (await ObtenerMarcasAsync()) ?? Array.Empty<string>();
        Input.CategoriasConocidas = (await ObtenerCategoriasAsync()) ?? Array.Empty<string>();
    }

    private async Task<IReadOnlyList<string>?> ObtenerMarcasAsync()
    {
        var resultado = await _api.ObtenerMarcasAsync();
        return resultado.Exito ? resultado.Valor?.ToList() : null;
    }

    private async Task<IReadOnlyList<string>?> ObtenerCategoriasAsync()
    {
        var resultado = await _api.ObtenerCategoriasAsync();
        return resultado.Exito ? resultado.Valor?.ToList() : null;
    }
}