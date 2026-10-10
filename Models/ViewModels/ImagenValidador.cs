namespace ApiPrimera.Models.ViewModels;

public static class ImagenValidador
{
    public const long TamanoMaximoBytes = 5 * 1024 * 1024;

    public static readonly IReadOnlyList<string> TiposPermitidos = new[]
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "image/avif"
    };

    public static bool EsTipoPermitido(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        var tipo = contentType.Split(';')[0].Trim();
        return TiposPermitidos.Contains(tipo, StringComparer.OrdinalIgnoreCase);
    }

    public static string? ValidarArchivo(IFormFile? archivo)
    {
        if (archivo is null)
        {
            return null;
        }

        if (archivo.Length == 0)
        {
            return "El archivo de imagen está vacío.";
        }

        if (archivo.Length > TamanoMaximoBytes)
        {
            return "La imagen supera el tamaño máximo permitido de 5 MB.";
        }

        if (!EsTipoPermitido(archivo.ContentType))
        {
            return "Formato no admitido. Usa JPG, PNG, WEBP, GIF o AVIF.";
        }

        return null;
    }

    public static string ContentTypeSeguro(IFormFile archivo) =>
        EsTipoPermitido(archivo.ContentType)
            ? archivo.ContentType.Split(';')[0].Trim()
            : "application/octet-stream";
}