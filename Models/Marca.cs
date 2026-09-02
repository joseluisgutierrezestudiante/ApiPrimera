namespace ApiPrimera.Models;

public class Marca
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    // Descripción de la marca
    public string Descripcion { get; set; } = string.Empty;
    // Porcentaje de descuento aplicable a los carros de esta marca (0-100)
    public decimal DiscountPercentage { get; set; }
    // Relación: una marca puede tener muchos carros
    public List<Carro> Carros { get; set; } = new();
}
