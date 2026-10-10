using ApiPrimera.Models;
using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ApiPrimera.Tests;

public class ProductoFormViewModelTests
{
    [Fact]
    public void ToProducto_normaliza_los_textos_y_no_copia_la_imagen()
    {
        var vm = new ProductoFormViewModel
        {
            Id = 7,
            Nombre = "  Toyota Corolla  ",
            Descripcion = "  Sedan  ",
            Marca = " Toyota ",
            Categoria = " Sedan ",
            Anio = 2024,
            Kilometraje = 1200,
            Combustible = " Gasolina ",
            Transmision = " Automatica ",
            Color = " Blanco ",
            Precio = 50_000_000m,
            PrecioOriginal = 55_000_000m,
            Stock = 4,
            Destacado = true,
            ImagenActual = "https://res.cloudinary.com/demo/image/upload/productos/x.jpg"
        };

        var producto = vm.ToProducto();

        Assert.Equal("Toyota Corolla", producto.Nombre);
        Assert.Equal("Toyota", producto.Marca);
        Assert.Equal("Sedan", producto.Categoria);
        Assert.Equal("Gasolina", producto.Combustible);
        Assert.Equal("Automatica", producto.Transmision);
        Assert.Equal("Blanco", producto.Color);
        Assert.Equal(7, producto.Id);
        Assert.True(producto.Destacado);
        Assert.Null(producto.ImagenUrl);
    }

    [Fact]
    public void ToProducto_convierte_null_en_cadena_vacia()
    {
        var vm = new ProductoFormViewModel { Nombre = "X", Marca = "Y" };

        var producto = vm.ToProducto();

        Assert.Equal(string.Empty, producto.Descripcion);
        Assert.Equal(string.Empty, producto.Categoria);
        Assert.Equal(string.Empty, producto.Combustible);
        Assert.Equal(string.Empty, producto.Transmision);
        Assert.Equal(string.Empty, producto.Color);
    }

    [Fact]
    public void FromProducto_mantiene_la_imagen_actual()
    {
        var producto = new Producto
        {
            Id = 3,
            Nombre = "Hilux",
            Marca = "Toyota",
            Precio = 80_000_000m,
            Stock = 2,
            ImagenUrl = "https://res.cloudinary.com/demo/image/upload/productos/hilux.jpg"
        };

        var vm = ProductoFormViewModel.FromProducto(producto);

        Assert.Equal(3, vm.Id);
        Assert.Equal("Hilux", vm.Nombre);
        Assert.Equal("https://res.cloudinary.com/demo/image/upload/productos/hilux.jpg", vm.ImagenActual);
    }

    [Fact]
    public void EsNuevo_es_true_solo_sin_id()
    {
        Assert.True(new ProductoFormViewModel().EsNuevo);
        Assert.False(new ProductoFormViewModel { Id = 1 }.EsNuevo);
    }

    [Fact]
    public void ValidarArchivo_rechaza_archivos_mas_grandes_que_el_limite()
    {
        var error = ImagenValidador.ValidarArchivo(
            ArchivoFalso((int)ImagenValidador.TamanoMaximoBytes + 1, "image/png"));

        Assert.NotNull(error);
        Assert.Contains("5 MB", error);
    }

    [Fact]
    public void ValidarArchivo_rechaza_el_archivo_vacio()
    {
        var error = ImagenValidador.ValidarArchivo(ArchivoFalso(0, "image/png"));

        Assert.NotNull(error);
        Assert.Contains("vacío", error);
    }

    [Fact]
    public void ValidarArchivo_sin_archivo_no_es_error()
    {
        Assert.Null(ImagenValidador.ValidarArchivo(null));
    }

    [Fact]
    public void ValidarArchivo_acepta_imagen_dentro_del_limite()
    {
        Assert.Null(ImagenValidador.ValidarArchivo(ArchivoFalso(1024, "image/png")));
    }

    [Fact]
    public void ValidarArchivo_rechaza_tipo_no_permitido()
    {
        var error = ImagenValidador.ValidarArchivo(ArchivoFalso(1024, "application/pdf"));

        Assert.NotNull(error);
        Assert.Contains("Formato", error);
    }

    [Fact]
    public void ValidarArchivo_rechaza_archivo_que_no_declara_una_imagen()
    {
        var error = ImagenValidador.ValidarArchivo(ArchivoFalso(1024, "application/x-msdownload", nombre: "virus.exe"));

        Assert.NotNull(error);
        Assert.Contains("Formato", error);
    }

    [Theory]
    [InlineData("image/png", true)]
    [InlineData("IMAGE/PNG", true)]
    [InlineData("image/jpeg; charset=binary", true)]
    [InlineData("image/svg+xml", false)]
    [InlineData("text/html", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void EsTipoPermitido_solo_acepta_la_lista(string? contentType, bool esperado)
    {
        Assert.Equal(esperado, ImagenValidador.EsTipoPermitido(contentType));
    }

    [Fact]
    public void ContentTypeSeguro_normaliza_y_fallbacka()
    {
        Assert.Equal("image/png", ImagenValidador.ContentTypeSeguro(ArchivoFalso(10, "image/png")));
        Assert.Equal(
            "application/octet-stream",
            ImagenValidador.ContentTypeSeguro(ArchivoFalso(10, "application/pdf")));
    }

    private static FormFile ArchivoFalso(int longitud, string contentType, string nombre = "foto.png")
    {
        var stream = new MemoryStream(new byte[Math.Max(longitud, 0)]);
        return new FormFile(stream, 0, longitud, "archivo", nombre)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}