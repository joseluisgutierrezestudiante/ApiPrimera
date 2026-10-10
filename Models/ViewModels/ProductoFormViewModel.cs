using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ApiPrimera.Models.ViewModels;

public class ProductoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre no puede superar 120 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción no puede superar 1000 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(60, ErrorMessage = "La marca no puede superar 60 caracteres.")]
    [Display(Name = "Marca")]
    public string Marca { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "La categoría no puede superar 60 caracteres.")]
    [Display(Name = "Categoría")]
    public string? Categoria { get; set; }

    [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100.")]
    [Display(Name = "Año")]
    public int Anio { get; set; } = DateTime.Now.Year;

    [Range(0, 1_000_000, ErrorMessage = "El kilometraje no puede ser negativo ni superar 1.000.000.")]
    [Display(Name = "Kilometraje")]
    public int Kilometraje { get; set; }

    [StringLength(40, ErrorMessage = "El combustible no puede superar 40 caracteres.")]
    [Display(Name = "Combustible")]
    public string? Combustible { get; set; }

    [StringLength(40, ErrorMessage = "La transmisión no puede superar 40 caracteres.")]
    [Display(Name = "Transmisión")]
    public string? Transmision { get; set; }

    [StringLength(40, ErrorMessage = "El color no puede superar 40 caracteres.")]
    [Display(Name = "Color")]
    public string? Color { get; set; }

    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "El precio no puede ser negativo.")]
    [Display(Name = "Precio")]
    public decimal Precio { get; set; }

    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "El precio original no puede ser negativo.")]
    [Display(Name = "Precio original")]
    public decimal? PrecioOriginal { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    [Display(Name = "Stock")]
    public int Stock { get; set; }

    [Display(Name = "Destacado")]
    public bool Destacado { get; set; }

    [Display(Name = "Imagen")]
    public IFormFile? Imagen { get; set; }

    public string? ImagenActual { get; set; }

    public bool QuitarImagen { get; set; }

    public IReadOnlyList<string> MarcasConocidas { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> CategoriasConocidas { get; set; } = Array.Empty<string>();

    public static readonly IReadOnlyList<string> Combustibles = new[]
    {
        "Gasolina", "Diésel", "Híbrido", "Eléctrico", "Gas"
    };

    public static readonly IReadOnlyList<string> Transmisiones = new[]
    {
        "Automática", "Manual"
    };

    public bool EsNuevo => Id == 0;

    public Producto ToProducto() => new()
    {
        Id = Id,
        Nombre = Nombre.Trim(),
        Descripcion = Descripcion?.Trim() ?? string.Empty,
        Marca = Marca.Trim(),
        Categoria = Categoria?.Trim() ?? string.Empty,
        Anio = Anio,
        Kilometraje = Kilometraje,
        Combustible = Combustible?.Trim() ?? string.Empty,
        Transmision = Transmision?.Trim() ?? string.Empty,
        Color = Color?.Trim() ?? string.Empty,
        Precio = Precio,
        PrecioOriginal = PrecioOriginal,
        Stock = Stock,
        Destacado = Destacado
    };

    public static ProductoFormViewModel FromProducto(Producto producto) => new()
    {
        Id = producto.Id,
        Nombre = producto.Nombre,
        Descripcion = producto.Descripcion,
        Marca = producto.Marca,
        Categoria = producto.Categoria,
        Anio = producto.Anio,
        Kilometraje = producto.Kilometraje,
        Combustible = producto.Combustible,
        Transmision = producto.Transmision,
        Color = producto.Color,
        Precio = producto.Precio,
        PrecioOriginal = producto.PrecioOriginal,
        Stock = producto.Stock,
        Destacado = producto.Destacado,
        ImagenActual = producto.ImagenUrl
    };
}