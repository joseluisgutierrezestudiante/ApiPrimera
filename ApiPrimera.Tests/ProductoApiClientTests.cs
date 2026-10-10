using System.Net;
using System.Text;
using System.Text.Json;
using ApiPrimera.Models;
using ApiPrimera.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ApiPrimera.Tests;

/// <summary>
/// Pruebas del cliente HTTP de la API. El servidor falso corre dentro del proceso
/// y registra lo que recibio, asi se puede verificar el verbo, la ruta y el cuerpo.
/// </summary>
public class ProductoApiClientTests : IDisposable
{
    private readonly HttpListener _servidor;
    private readonly HttpClient _http;
    private readonly ProductoApiClient _client;
    private readonly CancellationTokenSource _cts = new();
    private readonly object _cerrojo = new();
    private readonly string _baseUrl;

    private readonly List<Peticion> _peticiones = new();

    private HttpStatusCode _codigoRespuesta = HttpStatusCode.OK;
    private string _cuerpoRespuesta = "{}";

    private sealed record Peticion(HttpMethod Metodo, string Ruta, string ContentType, byte[] Cuerpo);

    public ProductoApiClientTests()
    {
        var puerto = PuertoLibre();
        _baseUrl = $"http://127.0.0.1:{puerto}/";

        _servidor = new HttpListener();
        _servidor.Prefixes.Add(_baseUrl);
        _servidor.Start();
        _ = AtenderAsync();

        _http = new HttpClient();
        _client = new ProductoApiClient(_http, new HttpContextAccessor(), ConfigConBaseUrl(_baseUrl));
    }

    [Fact]
    public async Task ObtenerPorIdAsync_deserializa_el_producto()
    {
        Responder(HttpStatusCode.OK, ProductoJson(id: 1, nombre: "Corolla"));

        var resultado = await _client.ObtenerPorIdAsync(1);

        Assert.True(resultado.Exito);
        Assert.Equal("Corolla", resultado.Valor!.Nombre);
        Assert.Equal(1, resultado.Valor.Id);
        Assert.False(resultado.NoEncontrado);
    }

    /// <summary>
    /// Si no hay BaseUrl configurado, el cliente debe apuntar al host de la
    /// peticion en curso. Asi las vistas funcionan sin depender del puerto fijo.
    /// </summary>
    [Fact]
    public async Task Sin_BaseUrl_configurado_usa_el_host_de_la_peticion_en_curso()
    {
        var url = new Uri(_baseUrl);
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        accessor.HttpContext!.Request.Scheme = url.Scheme;
        accessor.HttpContext!.Request.Host = new HostString(url.Host, url.Port);

        var client = new ProductoApiClient(new HttpClient(), accessor, ConfigConBaseUrl(""));
        Responder(HttpStatusCode.OK, ProductoJson(id: 1, nombre: "DesdeElHost"));

        var resultado = await client.ObtenerPorIdAsync(1);

        Assert.True(resultado.Exito);
        Assert.Equal("DesdeElHost", resultado.Valor!.Nombre);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_marca_no_encontrado_sin_error()
    {
        Responder(HttpStatusCode.NotFound);

        var resultado = await _client.ObtenerPorIdAsync(99);

        Assert.False(resultado.Exito);
        Assert.True(resultado.NoEncontrado);
        Assert.Null(resultado.Error);
    }

    [Fact]
    public async Task ObtenerPorIdAsync_propaga_el_mensaje_de_error_de_la_api()
    {
        Responder(HttpStatusCode.BadRequest, "\"El nombre es obligatorio.\"");

        var resultado = await _client.ObtenerPorIdAsync(1);

        Assert.False(resultado.Exito);
        Assert.Equal("El nombre es obligatorio.", resultado.Error);
    }

    [Fact]
    public async Task ObtenerTodosAsync_agrega_los_filtros_escapados()
    {
        Responder(HttpStatusCode.OK, "[]");

        var resultado = await _client.ObtenerTodosAsync("corolla azul", "Toyota", "SUV", "precio-asc");

        Assert.True(resultado.Exito);

        var peticion = Unica(p => p.Ruta.StartsWith("/api/producto?"));
        Assert.Contains("busqueda=corolla%20azul", peticion.Ruta);
        Assert.Contains("marca=Toyota", peticion.Ruta);
        Assert.Contains("categoria=SUV", peticion.Ruta);
        Assert.Contains("orden=precio-asc", peticion.Ruta);
    }

    [Fact]
    public async Task ObtenerTodosAsync_omite_los_parametros_vacios()
    {
        Responder(HttpStatusCode.OK, "[]");

        var resultado = await _client.ObtenerTodosAsync(busqueda: "   ", marca: null);

        Assert.True(resultado.Exito);
        Assert.Equal("/api/producto", Unica(p => p.Metodo == HttpMethod.Get).Ruta);
    }

    [Fact]
    public async Task ObtenerMarcasAsync_devuelve_las_marcas()
    {
        Responder(HttpStatusCode.OK, "[\"Honda\",\"Toyota\"]");

        var resultado = await _client.ObtenerMarcasAsync();

        Assert.True(resultado.Exito);
        Assert.Equal(new[] { "Honda", "Toyota" }, resultado.Valor!.ToArray());
        Assert.Equal("/api/producto/filtros/marcas", Unica(p => true).Ruta);
    }

    [Fact]
    public async Task ObtenerCategoriasAsync_devuelve_las_categorias()
    {
        Responder(HttpStatusCode.OK, "[\"Sedan\"]");

        var resultado = await _client.ObtenerCategoriasAsync();

        Assert.True(resultado.Exito);
        Assert.Equal(new[] { "Sedan" }, resultado.Valor!.ToArray());
    }

    [Fact]
    public async Task CrearAsync_envia_json_y_devuelve_el_producto_creado()
    {
        Responder(HttpStatusCode.Created, ProductoJson(id: 9, nombre: "Nuevo"));

        var resultado = await _client.CrearAsync(new Producto { Nombre = "Nuevo", Marca = "Kia" });

        Assert.True(resultado.Exito);
        Assert.Equal(9, resultado.Valor!.Id);

        var peticion = Unica(p => p.Metodo == HttpMethod.Post);
        Assert.Equal("/api/producto", peticion.Ruta);
        Assert.Contains("application/json", peticion.ContentType);
        Assert.Contains("Nuevo", Encoding.UTF8.GetString(peticion.Cuerpo));
    }

    [Fact]
    public async Task ActualizarAsync_trata_el_204_sin_cuerpo()
    {
        Responder(HttpStatusCode.NoContent);

        var producto = new Producto { Id = 5, Nombre = "Editado", Marca = "Kia" };
        var resultado = await _client.ActualizarAsync(producto);

        Assert.True(resultado.Exito);
        Assert.Equal("Editado", resultado.Valor!.Nombre);
        Assert.Equal("/api/producto/5", Unica(p => p.Metodo == HttpMethod.Put).Ruta);
    }

    [Fact]
    public async Task ActualizarAsync_propaga_el_error_de_validacion()
    {
        Responder(HttpStatusCode.BadRequest, "\"La marca es obligatoria.\"");

        var resultado = await _client.ActualizarAsync(new Producto { Id = 5, Nombre = "Sin marca" });

        Assert.False(resultado.Exito);
        Assert.Equal("La marca es obligatoria.", resultado.Error);
    }

    [Fact]
    public async Task SubirImagenAsync_envia_multipart_con_el_content_type_real()
    {
        Responder(HttpStatusCode.OK, ProductoJson(id: 3, imagen: "https://cdn/x.jpg"));

        await using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var resultado = await _client.SubirImagenAsync(3, stream, "foto.png", "image/png");

        Assert.True(resultado.Exito);
        Assert.Equal("https://cdn/x.jpg", resultado.Valor!.ImagenUrl);

        var peticion = Unica(p => p.Metodo == HttpMethod.Post);
        Assert.Equal("/api/producto/3/imagen", peticion.Ruta);

        // El content type de la parte va dentro del cuerpo multipart.
        Assert.Contains("multipart/form-data", peticion.ContentType);

        var cuerpo = Encoding.UTF8.GetString(peticion.Cuerpo);
        Assert.Contains("archivo", cuerpo);
        Assert.Contains("foto.png", cuerpo);
        Assert.Contains("image/png", cuerpo);
    }

    [Fact]
    public async Task EliminarAsync_devuelve_ok_en_un_borrado_correcto()
    {
        Responder(HttpStatusCode.NoContent);

        var resultado = await _client.EliminarAsync(2);

        Assert.True(resultado.Exito);
        Assert.True(resultado.Valor);
        Assert.Equal("/api/producto/2", Unica(p => p.Metodo == HttpMethod.Delete).Ruta);
    }

    [Fact]
    public async Task EliminarImagenAsync_usa_el_endpoint_de_imagen()
    {
        Responder(HttpStatusCode.NoContent);

        var resultado = await _client.EliminarImagenAsync(2);

        Assert.True(resultado.Exito);
        Assert.Equal("/api/producto/2/imagen", Unica(p => p.Metodo == HttpMethod.Delete).Ruta);
    }

    [Fact]
    public async Task EliminarAsync_marca_no_encontrado()
    {
        Responder(HttpStatusCode.NotFound);

        var resultado = await _client.EliminarAsync(404);

        Assert.True(resultado.NoEncontrado);
    }

    [Fact]
    public async Task Las_llamadas_reportan_error_si_la_api_no_responde()
    {
        var client = new ProductoApiClient(
            new HttpClient { Timeout = TimeSpan.FromMilliseconds(200) },
            new HttpContextAccessor(),
            ConfigConBaseUrl("http://127.0.0.1:1/"));

        var resultado = await client.ObtenerTodosAsync();

        Assert.False(resultado.Exito);
        Assert.False(resultado.NoEncontrado);
        Assert.NotNull(resultado.Error);
    }

    private Peticion Unica(Func<Peticion, bool> condicion)
    {
        List<Peticion> encontradas;
        lock (_cerrojo)
        {
            encontradas = _peticiones.Where(condicion).ToList();
        }

        Assert.Single(encontradas);
        return encontradas[0];
    }

    private void Responder(HttpStatusCode codigo, string cuerpo = "")
    {
        lock (_cerrojo)
        {
            _codigoRespuesta = codigo;
            _cuerpoRespuesta = cuerpo;
        }
    }

    private async Task AtenderAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            HttpListenerContext contexto;
            try
            {
                contexto = await _servidor.GetContextAsync();
            }
            catch
            {
                return;
            }

            using var memoria = new MemoryStream();
            await contexto.Request.InputStream.CopyToAsync(memoria);

            HttpStatusCode codigo;
            string cuerpo;
            lock (_cerrojo)
            {
                codigo = _codigoRespuesta;
                cuerpo = _cuerpoRespuesta;

                _peticiones.Add(new Peticion(
                    new HttpMethod(contexto.Request.HttpMethod),
                    contexto.Request.Url!.AbsolutePath + contexto.Request.Url.Query,
                    contexto.Request.ContentType ?? string.Empty,
                    memoria.ToArray()));
            }

            var bytes = Encoding.UTF8.GetBytes(cuerpo);
            contexto.Response.StatusCode = (int)codigo;
            contexto.Response.ContentType = "application/json";
            contexto.Response.ContentLength64 = bytes.Length;

            if (bytes.Length > 0)
            {
                await contexto.Response.OutputStream.WriteAsync(bytes);
            }

            contexto.Response.Close();
        }
    }

    private static int PuertoLibre()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var puerto = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return puerto;
    }

    private static string ProductoJson(int id, string nombre = "Producto", string? imagen = null) =>
        JsonSerializer.Serialize(new
        {
            id,
            nombre,
            descripcion = "Descripcion",
            marca = "Toyota",
            categoria = "Sedan",
            anio = 2024,
            kilometraje = 1000,
            combustible = "Gasolina",
            transmision = "Automatica",
            color = "Blanco",
            precio = 50_000_000m,
            precioOriginal = (decimal?)null,
            stock = 3,
            destacado = false,
            imagenUrl = imagen
        });

    private static IConfiguration ConfigConBaseUrl(string url) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiAutoconsumida:BaseUrl"] = url })
            .Build();

    public void Dispose()
    {
        _cts.Cancel();
        _http.Dispose();

        try
        {
            _servidor.Stop();
            _servidor.Close();
        }
        catch
        {
            // el listener ya estaba cerrado
        }

        GC.SuppressFinalize(this);
    }
}