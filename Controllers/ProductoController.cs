using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiPrimera.Interfaces;
using ApiPrimera.Models;

namespace ApiPrimera.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductoController : ControllerBase
{
    private const long TamanoMaximoBytes = 5 * 1024 * 1024;

    // Allow-list: no confiamos solo en el Content-Type que declara el cliente.
    private static readonly HashSet<string> TiposImagenPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "image/avif"
    };

    private readonly IProductoRepository _repo;
    private readonly ICloudinaryService _cloudinary;

    public ProductoController(IProductoRepository repo, ICloudinaryService cloudinary)
    {
        _repo = repo;
        _cloudinary = cloudinary;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetAll(
        [FromQuery] string? busqueda,
        [FromQuery] string? marca,
        [FromQuery] string? categoria,
        [FromQuery] string? orden)
    {
        var lista = await _repo.BuscarAsync(busqueda, marca, categoria, orden);
        return Ok(lista);
    }

    [HttpGet("filtros/marcas")]
    public async Task<ActionResult<IEnumerable<string>>> GetMarcas()
    {
        return Ok(await _repo.GetMarcasAsync());
    }

    [HttpGet("filtros/categorias")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategorias()
    {
        return Ok(await _repo.GetCategoriasAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Producto>> Get(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p == null) return NotFound();
        return Ok(p);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Producto>> Create([FromBody] Producto producto)
    {
        // validaciones básicas
        if (string.IsNullOrWhiteSpace(producto.Nombre)) return BadRequest("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(producto.Marca)) return BadRequest("La marca es obligatoria.");
        if (producto.Precio < 0) return BadRequest("El precio no puede ser negativo.");
        if (producto.Stock < 0) return BadRequest("El stock no puede ser negativo.");
        if (producto.PrecioOriginal is < 0) return BadRequest("El precio original no puede ser negativo.");

        var created = await _repo.CreateAsync(producto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Producto producto)
    {
        if (id != producto.Id) return BadRequest();
        if (string.IsNullOrWhiteSpace(producto.Nombre)) return BadRequest("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(producto.Marca)) return BadRequest("La marca es obligatoria.");
        if (producto.Precio < 0) return BadRequest("El precio no puede ser negativo.");
        if (producto.Stock < 0) return BadRequest("El stock no puede ser negativo.");
        if (producto.PrecioOriginal is < 0) return BadRequest("El precio original no puede ser negativo.");

        var ok = await _repo.UpdateAsync(producto);
        if (!ok) return NotFound();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    // Una sola accion para las dos variantes. Antes eran dos acciones que solo se
    // diferenciaban por [Consumes]: un Content-Type distinto launchaba
    // AmbiguousMatchException (HTTP 500) en lugar de un 415 coherente.
    [Authorize]
    [HttpPost("{id}/imagen")]
    public async Task<IActionResult> SubirImagen(int id)
    {
        if (!Request.HasFormContentType)
        {
            var request = await LeerJsonAsync();
            if (request is null)
            {
                return Unsupported();
            }

            if (string.IsNullOrWhiteSpace(request.ImagenBase64))
            {
                return BadRequest("Debes enviar el campo 'imagenBase64' con la imagen.");
            }

            return await GuardarImagenAsync(id, request.ImagenBase64);
        }

        if (Request.ContentLength is > TamanoMaximoBytes)
        {
            return BadRequest("La imagen supera el tamaño máximo permitido de 5 MB.");
        }

        var form = await Request.ReadFormAsync();
        var archivo = form.Files.FirstOrDefault();
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest("No se recibió ningún archivo.");
        }

        if (!EsTipoImagenPermitido(archivo.ContentType))
        {
            return Unsupported();
        }

        if (archivo.Length > TamanoMaximoBytes)
        {
            return BadRequest("La imagen supera el tamaño máximo permitido de 5 MB.");
        }

        await using var stream = archivo.OpenReadStream();
        return await GuardarImagenAsync(id, stream, archivo.FileName);
    }

    private async Task<ProductoImagenRequest?> LeerJsonAsync()
    {
        if (Request.ContentLength is > TamanoMaximoBytes)
        {
            return null;
        }

        try
        {
            return await Request.ReadFromJsonAsync<ProductoImagenRequest>();
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return null;
        }
    }

    private static bool EsTipoImagenPermitido(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        var tipo = contentType.Split(';')[0].Trim();

        return TiposImagenPermitidos.Contains(tipo);
    }

    private ObjectResult Unsupported() => StatusCode(
        StatusCodes.Status415UnsupportedMediaType,
        "Formato no admitido. Envía multipart/form-data con el campo 'archivo' o application/json con 'imagenBase64'.");

    [Authorize]
    [HttpDelete("{id}/imagen")]
    public async Task<IActionResult> EliminarImagen(int id)
    {
        var producto = await _repo.GetByIdAsync(id);
        if (producto == null) return NotFound();

        if (string.IsNullOrWhiteSpace(producto.ImagenUrl))
        {
            return BadRequest("El producto no tiene una imagen almacenada.");
        }

        try
        {
            await _cloudinary.DeleteImageByUrlAsync(producto.ImagenUrl);
        }
        catch (Exception)
        {
            // Si Cloudinary falla, igual liberamos la referencia en la base de datos.
        }

        await _repo.SetImagenUrlAsync(id, null);
        return NoContent();
    }

    private async Task<IActionResult> GuardarImagenAsync(int id, string base64Image)
    {
        string imagenUrl;
        try
        {
            imagenUrl = await _cloudinary.UploadImageAsync(base64Image);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
        }

        return await ActualizarImagenUrlAsync(id, imagenUrl);
    }

    private async Task<IActionResult> GuardarImagenAsync(int id, Stream imageStream, string fileName)
    {
        string imagenUrl;
        try
        {
            imagenUrl = await _cloudinary.UploadImageAsync(imageStream, fileName);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, ex.Message);
        }

        return await ActualizarImagenUrlAsync(id, imagenUrl);
    }

    private async Task<IActionResult> ActualizarImagenUrlAsync(int id, string imagenUrl)
    {
        var producto = await _repo.GetByIdAsync(id);
        if (producto == null) return NotFound();

        var imagenAnterior = producto.ImagenUrl;
        var ok = await _repo.SetImagenUrlAsync(id, imagenUrl);
        if (!ok) return NotFound();

        if (!string.IsNullOrWhiteSpace(imagenAnterior) && imagenAnterior != imagenUrl)
        {
            try
            {
                await _cloudinary.DeleteImageByUrlAsync(imagenAnterior);
            }
            catch (Exception)
            {
                // La imagen nueva ya quedó guardada; no bloqueamos la respuesta.
            }
        }

        producto.ImagenUrl = imagenUrl;
        return Ok(producto);
    }
}
