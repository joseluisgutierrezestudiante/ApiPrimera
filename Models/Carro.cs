namespace ApiPrimera.Models;

public class Carro
{
    public int Id { get; set; }
    public string Modelo { get; set; } = string.Empty;
    // Precio original sin descuento
    public decimal Precio { get; set; }
    // Precio calculado aplicando el descuento de la marca
    public decimal PrecioConDescuento { get; set; }
    // Relación: cada carro pertenece a una única marca
    public int MarcaId { get; set; }
    // Propiedad de navegación opcional para respuestas (no obligatoria en memoria)
    public Marca? Marca { get; set; }
}
