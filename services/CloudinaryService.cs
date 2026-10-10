using ApiPrimera.Configuration;
using ApiPrimera.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace ApiPrimera.Services;

public class CloudinaryService : ICloudinaryService
{
    internal const string CarpetaProductos = "productos";

    private readonly CloudinarySettings _settings;
    private Cloudinary? _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> settings)
    {
        _settings = settings.Value;
    }

    private Cloudinary Cliente => _cloudinary ??= CrearCliente();

    private Cloudinary CrearCliente()
    {
        if (!_settings.EstaConfigurado)
        {
            throw new InvalidOperationException(
                "Falta configurar CloudinarySettings (CloudName, ApiKey, ApiSecret) en los user-secrets.");
        }

        return new Cloudinary(new Account(
            _settings.CloudName,
            _settings.ApiKey,
            _settings.ApiSecret));
    }

    public Task<string> UploadImageAsync(string base64Image)
    {
        if (string.IsNullOrWhiteSpace(base64Image))
        {
            throw new ArgumentException("La imagen no puede estar vacía.", nameof(base64Image));
        }

        var base64Data = base64Image;
        var commaIndex = base64Image.IndexOf(',');
        if (commaIndex >= 0)
        {
            base64Data = base64Image[(commaIndex + 1)..];
        }

        byte[] imageBytes;
        try
        {
            imageBytes = Convert.FromBase64String(base64Data);
        }
        catch (FormatException)
        {
            throw new ArgumentException("El formato de la imagen Base64 no es válido.", nameof(base64Image));
        }

        return UploadImageAsync(new MemoryStream(imageBytes), "producto");
    }

    public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, imageStream),
            Folder = CarpetaProductos,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await Cliente.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            throw new InvalidOperationException($"Error al subir la imagen a Cloudinary: {result.Error.Message}");
        }

        return result.SecureUrl.ToString();
    }

    public async Task<bool> DeleteImageByUrlAsync(string imageUrl)
    {
        var publicId = ObtenerPublicIdDesdeUrl(imageUrl);
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return false;
        }

        var result = await Cliente.DestroyAsync(new DeletionParams(publicId));

        return result.Error == null && string.Equals(result.Result, "ok", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extrae el public_id de una URL de Cloudinary. Ejemplo:
    /// https://res.cloudinary.com/demo/image/upload/productos/abc123.jpg -> productos/abc123
    /// </summary>
    internal static string? ObtenerPublicIdDesdeUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var uploadMarker = "/upload/";
        var markerIndex = imageUrl.IndexOf(uploadMarker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return null;
        }

        var publicIdWithExtension = imageUrl[(markerIndex + uploadMarker.Length)..];

        var queryIndex = publicIdWithExtension.IndexOf('?');
        if (queryIndex >= 0)
        {
            publicIdWithExtension = publicIdWithExtension[..queryIndex];
        }

        var extensionIndex = publicIdWithExtension.LastIndexOf('.');
        if (extensionIndex > publicIdWithExtension.LastIndexOf('/'))
        {
            publicIdWithExtension = publicIdWithExtension[..extensionIndex];
        }

        return publicIdWithExtension.Trim('/');
    }
}
