namespace ApiPrimera.Models.ViewModels;

public class CarritoItemViewModel
{
    public required Models.Producto Producto { get; init; }

    public int Cantidad { get; init; }

    public bool SinStock => Producto.Stock <= 0;

    public int CantidadDisponible =>
        SinStock ? 0 : Math.Min(Cantidad, Producto.Stock);

    public decimal Subtotal => Producto.Precio * Cantidad;
}