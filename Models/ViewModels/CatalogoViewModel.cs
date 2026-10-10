using ApiPrimera.Models;

namespace ApiPrimera.Models.ViewModels;

public class CatalogoViewModel
{
    public IReadOnlyList<Producto> Productos { get; init; } = Array.Empty<Producto>();

    public IReadOnlyList<string> Marcas { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Categorias { get; init; } = Array.Empty<string>();

    public string? Busqueda { get; init; }

    public string? Marca { get; init; }

    public string? Categoria { get; init; }

    public string? Orden { get; init; }

    public string? Error { get; init; }

    public bool HayFiltros =>
        !string.IsNullOrWhiteSpace(Busqueda) ||
        !string.IsNullOrWhiteSpace(Marca) ||
        !string.IsNullOrWhiteSpace(Categoria);

    public int Total => Productos.Count;
}