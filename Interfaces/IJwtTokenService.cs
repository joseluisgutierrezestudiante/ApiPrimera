using Microsoft.AspNetCore.Identity;

namespace ApiPrimera.Interfaces;

/// <summary>
/// Emite access tokens JWT de vida corta y administra refresh tokens
/// con rotación (el token usado deja de servir y nace uno nuevo).
/// </summary>
public interface IJwtTokenService
{
    /// <summary>Firma un access token JWT con la expiración corta configurada (15 min).</summary>
    (string Token, DateTimeOffset ExpiraEn) CrearAccessToken(IdentityUser usuario);

    /// <summary>Genera y persiste (hasheado) un nuevo refresh token para el usuario.</summary>
    Task<string> EmitirRefreshTokenAsync(IdentityUser usuario);

    /// <summary>
    /// Valida el refresh token y lo rota: devuelve el usuario y el token nuevo.
    /// Devuelve null si el token es inválido, expirado o ya fue consumido.
    /// </summary>
    Task<(IdentityUser Usuario, string RefreshToken)?> RotarRefreshTokenAsync(string refreshToken);

    /// <summary>Elimina un refresh token puntual (logout sin cookie de sesión).</summary>
    Task RevocarRefreshTokenAsync(string refreshToken);

    /// <summary>Elimina todos los refresh tokens del usuario (logout global).</summary>
    Task RevocarTodosLosRefreshTokensAsync(IdentityUser usuario);
}
