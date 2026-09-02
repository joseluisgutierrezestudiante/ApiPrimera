namespace ApiPrimera.Models;

// Clase Carro simplificada para principiantes
public class Carro
{
    public int Id { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    // Precio original sin descuento
    public decimal Precio { get; set; }
    // Precio calculado aplicando el descuento de la marca
    public decimal PrecioConDescuento { get; set; }
    // Relación: cada carro pertenece a una única marca (Id)
    public int MarcaId { get; set; }
}
