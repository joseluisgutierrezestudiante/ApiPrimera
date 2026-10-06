using ApiPrimera.Configuration;
using ApiPrimera.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace ApiPrimera.Tests;

public class CloudinaryServiceTests
{
    [Theory]
    [InlineData("https://res.cloudinary.com/demo/image/upload/productos/abc123.jpg", "productos/abc123")]
    [InlineData("https://res.cloudinary.com/demo/image/upload/productos/nested/car1.png", "productos/nested/car1")]
    [InlineData("https://res.cloudinary.com/demo/image/upload/productos/abc123.jpg?v=1", "productos/abc123")]
    [InlineData("https://res.cloudinary.com/demo/image/upload/abc123", "abc123")]
    public void ObtenerPublicIdDesdeUrl_extrae_el_identificador(string url, string esperado)
    {
        Assert.Equal(esperado, CloudinaryService.ObtenerPublicIdDesdeUrl(url));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://thumb.wikimedia.org/wikipedia/commons/Toyota.jpg")]
    [InlineData("no-es-una-url")]
    public void ObtenerPublicIdDesdeUrl_devuelve_null_si_no_hay_marcador_de_subida(string url)
    {
        Assert.Null(CloudinaryService.ObtenerPublicIdDesdeUrl(url));
    }

    [Fact]
    public void CarpetaProductos_es_productos()
    {
        Assert.Equal("productos", CloudinaryService.CarpetaProductos);
    }

    [Fact]
    public async Task Cliente_lanza_si_falta_configuracion()
    {
        var service = new CloudinaryService(Options.Create(new CloudinarySettings()));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UploadImageAsync(new MemoryStream(new byte[] { 1, 2, 3 }), "foto.jpg"));

        Assert.Contains("Cloudinary", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    public async Task UploadImageAsync_base64_rechaza_vacio(string base64)
    {
        var service = new CloudinaryService(Options.Create(new CloudinarySettings()));

        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadImageAsync(base64));
    }

    [Theory]
    [InlineData("no-es-base64-valido!!!")]
    [InlineData("####")]
    public async Task UploadImageAsync_base64_rechaza_formato_invalido(string base64)
    {
        var service = new CloudinaryService(Options.Create(Configurado()));

        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadImageAsync(base64));
    }

    [Fact]
    public async Task DeleteImageByUrlAsync_no_llama_a_cloudinary_si_la_url_no_es_suya()
    {
        var service = new CloudinaryService(Options.Create(new CloudinarySettings()));

        var resultado = await service.DeleteImageByUrlAsync("https://thumb.wikimedia.org/wikipedia/commons/Toyota.jpg");

        Assert.False(resultado);
    }

    private static CloudinarySettings Configurado() => new()
    {
        CloudName = "demo",
        ApiKey = "clave",
        ApiSecret = "secreto"
    };
}