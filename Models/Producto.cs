using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPrimera.Models;

public class Producto
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Marca { get; set; } = string.Empty;

    [StringLength(60)]
    public string Categoria { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Anio { get; set; }

    [Range(0, 1_000_000)]
    public int Kilometraje { get; set; }

    [StringLength(40)]
    public string Combustible { get; set; } = string.Empty;

    [StringLength(40)]
    public string Transmision { get; set; } = string.Empty;

    [StringLength(40)]
    public string Color { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public decimal? PrecioOriginal { get; set; }

    public int Stock { get; set; }

    public bool Destacado { get; set; }

    public string? ImagenUrl { get; set; }

    [NotMapped]
    public int DescuentoPorcentaje =>
        PrecioOriginal is > 0 && PrecioOriginal.Value > Precio
            ? (int)Math.Round((PrecioOriginal.Value - Precio) / PrecioOriginal.Value * 100m)
            : 0;

    [NotMapped]
    public bool Agotado => Stock <= 0;
}