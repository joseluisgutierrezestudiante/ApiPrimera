namespace ApiPrimera.Configuration;

/// <summary>
/// Opciones de autenticación JWT.
/// El secreto NUNCA se versiona: se lee de la variable de entorno
/// Jwt__SecretKey (o de user-secrets con la clave "Jwt:SecretKey").
/// </summary>
public class JwtSettings
{
    public const string Seccion = "Jwt";

    /// <summary>Clave simétrica para firmar los tokens (HS256). Mínimo 32 caracteres.</summary>
    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = "ApiPrimera";

    public string Audience { get; set; } = "AutoPrime";

    /// <summary>Vida corta del access token: 15 minutos.</summary>
    public int ExpirationMinutes { get; set; } = 15;

    /// <summary>Vida del refresh token: 7 días.</summary>
    public int RefreshTokenDays { get; set; } = 7;

    public bool EstaConfigurada =>
        !string.IsNullOrWhiteSpace(SecretKey) && SecretKey.Length >= 32;
}
