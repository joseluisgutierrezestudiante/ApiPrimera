using ApiPrimera.Configuration;
using ApiPrimera.DB;
using ApiPrimera.Data;
using ApiPrimera.Interfaces;
using ApiPrimera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ApiPrimera.Services;

public class ProductoSeedService : IProductoSeedService
{
    private readonly AppDbContext _context;
    private readonly ICloudinaryService _cloudinary;
    private readonly CloudinarySettings _cloudinarySettings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProductoSeedService> _logger;

    public ProductoSeedService(
        AppDbContext context,
        ICloudinaryService cloudinary,
        IOptions<CloudinarySettings> cloudinarySettings,
        IHttpClientFactory httpClientFactory,
        ILogger<ProductoSeedService> logger)
    {
        _context = context;
        _cloudinary = cloudinary;
        _cloudinarySettings = cloudinarySettings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<int> SemearAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Producto.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("El catalogo ya tiene productos: no se siembra nada.");
            return 0;
        }

        var usarCloudinary = _cloudinarySettings.EstaConfigurado;
        if (usarCloudinary)
        {
            _logger.LogInformation("Sembrando catalogo y subiendo imagenes a Cloudinary...");
        }
        else
        {
            _logger.LogWarning(
                "Cloudinary no esta configurado: se guardara la URL de origen de cada imagen. " +
                "Configura CloudName, ApiKey y ApiSecret para servirlas desde tu cuenta.");
        }

        var http = _httpClientFactory.CreateClient("catalogo-origenes");
        http.DefaultRequestHeaders.UserAgent.ParseAdd(CatalogoProductos.AgenteHttp);

        var productos = new List<Producto>(CatalogoProductos.Semillas.Count);

        foreach (var semilla in CatalogoProductos.Semillas)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var imagenUrl = await ResolverImagenAsync(http, semilla.ImagenOrigen, usarCloudinary, cancellationToken);

            productos.Add(new Producto
            {
                Nombre = semilla.Nombre,
                Descripcion = semilla.Descripcion,
                Marca = semilla.Marca,
                Categoria = semilla.Categoria,
                Anio = semilla.Anio,
                Kilometraje = semilla.Kilometraje,
                Combustible = semilla.Combustible,
                Transmision = semilla.Transmision,
                Color = semilla.Color,
                Precio = semilla.Precio,
                PrecioOriginal = semilla.PrecioOriginal,
                Stock = semilla.Stock,
                Destacado = semilla.Destacado,
                ImagenUrl = imagenUrl
            });
        }

        await _context.Producto.AddRangeAsync(productos, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Catalogo sembrado con {Cantidad} productos.", productos.Count);
        return productos.Count;
    }

    private async Task<string?> ResolverImagenAsync(
        HttpClient http,
        string imagenOrigen,
        bool usarCloudinary,
        CancellationToken cancellationToken)
    {
        if (!usarCloudinary || EsImagenDeCloudinary(imagenOrigen))
        {
            return imagenOrigen;
        }

        try
        {
            var bytes = await http.GetByteArrayAsync(imagenOrigen, cancellationToken);

            await using var stream = new MemoryStream(bytes);
            return await _cloudinary.UploadImageAsync(stream, "catalogo-productos");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No se pudo subir la imagen {Url} a Cloudinary: se usara la URL de origen.",
                imagenOrigen);
            return imagenOrigen;
        }
    }

    private static bool EsImagenDeCloudinary(string imageUrl) =>
        imageUrl.StartsWith("https://res.cloudinary.com/", StringComparison.OrdinalIgnoreCase);
}