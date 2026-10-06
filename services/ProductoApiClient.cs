using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ApiPrimera.Interfaces;
using ApiPrimera.Models;

namespace ApiPrimera.Services;

/// <summary>
/// Cliente HTTP de la API de productos. Las vistas lo usan para leer y escribir,
/// de modo que el flujo producto -> imagen -> Cloudinary -> URL -> API -> frontend
/// queda completo y pasa siempre por los mismos endpoints.
/// </summary>
public class ProductoApiClient : IProductoApiClient
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Uri? _baseUrlConfigurada;

    public ProductoApiClient(
        HttpClient http,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _http = http;
        _httpContextAccessor = httpContextAccessor;

        var baseUrl = configuration["ApiAutoconsumida:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            _baseUrlConfigurada = new Uri(baseUrl);
            _http.BaseAddress = _baseUrlConfigurada;
        }

        _http.Timeout = TimeSpan.FromSeconds(30);
    }

    public Task<ResultadoApi<IEnumerable<Producto>>> ObtenerTodosAsync(
        string? busqueda = null,
        string? marca = null,
        string? categoria = null,
        string? orden = null)
    {
        var query = ConstruirQuery(busqueda, marca, categoria, orden);
        return EjecutarGet<IEnumerable<Producto>>($"/api/producto{query}");
    }

    public Task<ResultadoApi<IEnumerable<string>>> ObtenerMarcasAsync()
    {
        return EjecutarGet<IEnumerable<string>>("/api/producto/filtros/marcas");
    }

    public Task<ResultadoApi<IEnumerable<string>>> ObtenerCategoriasAsync()
    {
        return EjecutarGet<IEnumerable<string>>("/api/producto/filtros/categorias");
    }

    public Task<ResultadoApi<Producto>> ObtenerPorIdAsync(int id)
    {
        return EjecutarGet<Producto>($"/api/producto/{id}");
    }

    public async Task<ResultadoApi<Producto>> CrearAsync(Producto producto)
    {
        try
        {
            using var peticion = CrearPeticion(HttpMethod.Post, "/api/producto");
            peticion.Content = JsonContent.Create(producto, options: OpcionesJson);

            using var respuesta = await _http.SendAsync(peticion);
            return await LeerProductoAsync(respuesta);
        }
        catch (Exception ex) when (EsErrorDeTransporte(ex))
        {
            return ResultadoApi<Producto>.Fail(MensajeDeTransporte(ex));
        }
    }

    public async Task<ResultadoApi<Producto>> ActualizarAsync(Producto producto)
    {
        try
        {
            using var peticion = CrearPeticion(HttpMethod.Put, $"/api/producto/{producto.Id}");
            peticion.Content = JsonContent.Create(producto, options: OpcionesJson);

            using var respuesta = await _http.SendAsync(peticion);

            // El PUT responde 204 sin cuerpo: devolvemos el estado enviado.
            return respuesta.StatusCode == HttpStatusCode.NoContent
                ? ResultadoApi<Producto>.Ok(producto)
                : await LeerProductoAsync(respuesta);
        }
        catch (Exception ex) when (EsErrorDeTransporte(ex))
        {
            return ResultadoApi<Producto>.Fail(MensajeDeTransporte(ex));
        }
    }

    public Task<ResultadoApi<bool>> EliminarAsync(int id)
    {
        return EjecutarSinCuerpo(HttpMethod.Delete, $"/api/producto/{id}");
    }

    public async Task<ResultadoApi<Producto>> SubirImagenAsync(
        int id,
        Stream contenido,
        string nombreArchivo,
        string contentType)
    {
        try
        {
            using var form = new MultipartFormDataContent();
            var archivo = new StreamContent(contenido);
            archivo.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(archivo, "archivo", nombreArchivo);

            using var peticion = CrearPeticion(HttpMethod.Post, $"/api/producto/{id}/imagen");
            peticion.Content = form;

            using var respuesta = await _http.SendAsync(peticion);
            return await LeerProductoAsync(respuesta);
        }
        catch (Exception ex) when (EsErrorDeTransporte(ex))
        {
            return ResultadoApi<Producto>.Fail(MensajeDeTransporte(ex));
        }
    }

    public Task<ResultadoApi<bool>> EliminarImagenAsync(int id)
    {
        return EjecutarSinCuerpo(HttpMethod.Delete, $"/api/producto/{id}/imagen");
    }

    private async Task<ResultadoApi<T>> EjecutarGet<T>(string ruta)
    {
        try
        {
            using var peticion = CrearPeticion(HttpMethod.Get, ruta);
            using var respuesta = await _http.SendAsync(peticion);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return ResultadoApi<T>.NotFound();
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                return ResultadoApi<T>.Fail(await LeerErrorAsync(respuesta));
            }

            var valor = await respuesta.Content.ReadFromJsonAsync<T>(OpcionesJson);

            return valor is null
                ? ResultadoApi<T>.Fail("La API devolvió una respuesta vacía.")
                : ResultadoApi<T>.Ok(valor);
        }
        catch (Exception ex) when (EsErrorDeTransporte(ex))
        {
            return ResultadoApi<T>.Fail(MensajeDeTransporte(ex));
        }
    }

    private async Task<ResultadoApi<bool>> EjecutarSinCuerpo(HttpMethod metodo, string ruta)
    {
        try
        {
            using var peticion = CrearPeticion(metodo, ruta);
            using var respuesta = await _http.SendAsync(peticion);

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
            {
                return ResultadoApi<bool>.NotFound();
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                return ResultadoApi<bool>.Fail(await LeerErrorAsync(respuesta));
            }

            return ResultadoApi<bool>.Ok(true);
        }
        catch (Exception ex) when (EsErrorDeTransporte(ex))
        {
            return ResultadoApi<bool>.Fail(MensajeDeTransporte(ex));
        }
    }

    private static async Task<ResultadoApi<Producto>> LeerProductoAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            return ResultadoApi<Producto>.NotFound();
        }

        if (!respuesta.IsSuccessStatusCode)
        {
            return ResultadoApi<Producto>.Fail(await LeerErrorAsync(respuesta));
        }

        var producto = await respuesta.Content.ReadFromJsonAsync<Producto>(OpcionesJson);

        return producto is null
            ? ResultadoApi<Producto>.Fail("La API devolvió una respuesta vacía.")
            : ResultadoApi<Producto>.Ok(producto);
    }

    private static async Task<string> LeerErrorAsync(HttpResponseMessage respuesta)
    {
        var cuerpo = await respuesta.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(cuerpo))
        {
            return $"La API respondió con el código {(int)respuesta.StatusCode}.";
        }

        try
        {
            using var documento = JsonDocument.Parse(cuerpo);
            if (documento.RootElement.ValueKind == JsonValueKind.String)
            {
                var mensaje = documento.RootElement.GetString();
                if (!string.IsNullOrWhiteSpace(mensaje))
                {
                    return mensaje;
                }
            }
        }
        catch (JsonException)
        {
            // La API no devolvio un JSON valido: usamos el cuerpo recortado.
        }

        return cuerpo.Length > 300 ? cuerpo[..300] : cuerpo;
    }

    /// <summary>
    /// Si no hay BaseAddress configurado, se llama a la propia app usando el host
    /// de la peticion en curso. Asi las vistas funcionan en cualquier puerto.
    /// </summary>
    private HttpRequestMessage CrearPeticion(HttpMethod metodo, string ruta)
    {
        var peticion = new HttpRequestMessage(metodo, ruta);

        if (_http.BaseAddress is null && Uri.TryCreate(UriBaseActual(), UriKind.Absolute, out var baseActual))
        {
            peticion.RequestUri = new Uri(baseActual, ruta);
        }

        return peticion;
    }

    private string? UriBaseActual()
    {
        if (_baseUrlConfigurada is not null)
        {
            return _baseUrlConfigurada.ToString();
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            return null;
        }

        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }

    private static bool EsErrorDeTransporte(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException or OperationCanceledException;

    private static string MensajeDeTransporte(Exception ex) =>
        ex is HttpRequestException or TaskCanceledException or OperationCanceledException
            ? $"No se pudo contactar la API: {ex.Message}"
            : ex.Message;

    private static string ConstruirQuery(string? busqueda, string? marca, string? categoria, string? orden)
    {
        var parametros = new List<string>();

        Agregar(parametros, "busqueda", busqueda);
        Agregar(parametros, "marca", marca);
        Agregar(parametros, "categoria", categoria);
        Agregar(parametros, "orden", orden);

        return parametros.Count == 0 ? string.Empty : "?" + string.Join("&", parametros);
    }

    private static void Agregar(List<string> parametros, string clave, string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return;
        parametros.Add($"{clave}={Uri.EscapeDataString(valor.Trim())}");
    }
}