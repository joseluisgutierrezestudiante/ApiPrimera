namespace ApiPrimera.Models;

// Clase simple para principiantes: solo campos básicos
public class Marca
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    // Porcentaje de descuento aplicable a los carros de esta marca (0-100)
    public decimal DiscountPercentage { get; set; }
}
