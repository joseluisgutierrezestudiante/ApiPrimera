namespace ApiPrimera.Interfaces;

public interface ICarritoService
{
    int Conteo { get; }

    IReadOnlyList<Models.ViewModels.LineaCarrito> Lineas { get; }

    void Agregar(int productoId, int cantidad = 1);

    void ActualizarCantidad(int productoId, int cantidad);

    void Quitar(int productoId);

    void Vaciar();
}