using ApiPrimera.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ApiPrimera.Tests;

public class CarritoServiceTests
{
    [Fact]
    public void Agregar_CreaLineaYReportaConteo()
    {
        var (servicio, _) = CrearServicio();

        servicio.Agregar(10);
        servicio.Agregar(20, 3);

        Assert.Equal(2, servicio.Lineas.Count);
        Assert.Equal(4, servicio.Conteo);
        Assert.Equal(3, servicio.Lineas.First(l => l.ProductoId == 20).Cantidad);
    }

    [Fact]
    public void Agregar_MismoProducto_IncrementaLaLinea()
    {
        var (servicio, _) = CrearServicio();

        servicio.Agregar(7);
        servicio.Agregar(7, 2);
        servicio.Agregar(7, 5);

        Assert.Single(servicio.Lineas);
        Assert.Equal(8, servicio.Conteo);
        Assert.Equal(8, servicio.Lineas[0].Cantidad);
    }

    [Fact]
    public void Agregar_CantidadInvalida_NoHaceNada()
    {
        var (servicio, _) = CrearServicio();

        servicio.Agregar(5, 0);
        servicio.Agregar(5, -2);

        Assert.Empty(servicio.Lineas);
        Assert.Equal(0, servicio.Conteo);
    }

    [Fact]
    public void ActualizarCantidad_ModificaElValor()
    {
        var (servicio, _) = CrearServicio();
        servicio.Agregar(3, 2);

        servicio.ActualizarCantidad(3, 4);

        Assert.Equal(4, servicio.Lineas[0].Cantidad);
    }

    [Fact]
    public void ActualizarCantidad_Cero_QuitaLaLinea()
    {
        var (servicio, _) = CrearServicio();
        servicio.Agregar(3, 2);

        servicio.ActualizarCantidad(3, 0);

        Assert.Empty(servicio.Lineas);
    }

    [Fact]
    public void Quitar_EliminaSoloElProductoIndicado()
    {
        var (servicio, _) = CrearServicio();
        servicio.Agregar(1);
        servicio.Agregar(2);

        servicio.Quitar(1);

        Assert.Single(servicio.Lineas);
        Assert.Equal(2, servicio.Lineas[0].ProductoId);
    }

    [Fact]
    public void Vaciar_DejaElCarritoSinLineas()
    {
        var (servicio, _) = CrearServicio();
        servicio.Agregar(1);
        servicio.Agregar(2);

        servicio.Vaciar();

        Assert.Empty(servicio.Lineas);
        Assert.Equal(0, servicio.Conteo);
    }

    [Fact]
    public void ElCarrito_SePersisteEnLaSesion()
    {
        var sesion = new MocoSesion();

        var primerServicio = new CarritoService(Acceso(sesion));
        primerServicio.Agregar(42, 3);
        primerServicio.Agregar(9, 1);

        var segundoServicio = new CarritoService(Acceso(sesion));

        Assert.Equal(2, segundoServicio.Lineas.Count);
        Assert.Equal(4, segundoServicio.Conteo);
        Assert.Equal(3, segundoServicio.Lineas.First(l => l.ProductoId == 42).Cantidad);
    }

    private static (CarritoService, MocoSesion) CrearServicio()
    {
        var sesion = new MocoSesion();
        return (new CarritoService(Acceso(sesion)), sesion);
    }

    private static HttpContextAccessor Acceso(MocoSesion sesion)
    {
        var contexto = new DefaultHttpContext
        {
            Session = sesion
        };
        return new HttpContextAccessor { HttpContext = contexto };
    }
}

public class MocoSesion : ISession
{
    private readonly Dictionary<string, byte[]> _datos = new();

    public string Id => "sesion-de-prueba";

    public bool IsAvailable => true;

    public IEnumerable<string> Keys => _datos.Keys;

    public void Clear() => _datos.Clear();

    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public void Remove(string key) => _datos.Remove(key);

    public void Set(string key, byte[] value) => _datos[key] = value;

    public bool TryGetValue(string key, out byte[] value) => _datos.TryGetValue(key, out value!);
}