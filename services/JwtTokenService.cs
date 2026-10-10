using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ApiPrimera.Configuration;
using ApiPrimera.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiPrimera.Services;

/// <summary>
/// Emite access tokens JWT de vida corta (15 minutos por defecto) y gestiona
/// refresh tokens con rotación de uso único.
///
/// Almacenamiento: los refresh tokens se guardan SIEMPRE hasheados (SHA-256)
/// como JSON en la tabla AspNetUserTokens, por lo que no se requieren cambios
/// de esquema ni migraciones, y el valor real solo existe en el cliente.
/// Se conservan hasta 5 sesiones simultáneas por usuario.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private const string Proveedor = "AutoPrime";
    private const string NombreRefreshTokens = "RefreshTokens";
    private const int MaximoRefreshTokens = 5;

    private readonly JwtSettings _opciones;
    private readonly UserManager<IdentityUser> _userManager;

    public JwtTokenService(IOptions<JwtSettings> opciones, UserManager<IdentityUser> userManager)
    {
        _opciones = opciones.Value;
        _userManager = userManager;
    }

    public (string Token, DateTimeOffset ExpiraEn) CrearAccessToken(IdentityUser usuario)
    {
        var ahora = DateTime.UtcNow;
        var expira = ahora.AddMinutes(_opciones.ExpirationMinutes);
        var email = usuario.Email ?? usuario.UserName ?? string.Empty;

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.NameIdentifier, usuario.Id),
            new(ClaimTypes.Name, email),
        };

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: ahora,
            expires: expira,
            signingCredentials: credenciales);

        var texto = new JwtSecurityTokenHandler().WriteToken(token);
        return (texto, new DateTimeOffset(expira, TimeSpan.Zero));
    }

    public async Task<string> EmitirRefreshTokenAsync(IdentityUser usuario)
    {
        // Formato "<userId>.<aleatorio>": el prefijo permite localizar al
        // usuario sin escanear la base; la firma real es el hash guardado.
        var token = CrearRefreshToken(usuario);
        var lista = await LeerAsync(usuario);
        lista.Add(NuevoRegistro(token));
        await GuardarAsync(usuario, Podar(lista));
        return token;
    }

    public async Task<(IdentityUser Usuario, string RefreshToken)?> RotarRefreshTokenAsync(string refreshToken)
    {
        var usuario = await LocalizarUsuarioAsync(refreshToken);
        if (usuario is null) return null;

        var hash = CalcularHash(refreshToken);
        var ahora = DateTimeOffset.UtcNow;
        var lista = await LeerAsync(usuario);

        // El token debe existir, estar vigente y coincidir: consumo único.
        var registro = lista.FirstOrDefault(r => r.Hash == hash && r.ExpiraEn > ahora);
        if (registro is null) return null;
        lista.Remove(registro);

        var nuevo = CrearRefreshToken(usuario);
        lista.Add(NuevoRegistro(nuevo));
        await GuardarAsync(usuario, Podar(lista));

        return (usuario, nuevo);
    }

    public async Task RevocarRefreshTokenAsync(string refreshToken)
    {
        var usuario = await LocalizarUsuarioAsync(refreshToken);
        if (usuario is null) return;

        var lista = await LeerAsync(usuario);
        if (lista.RemoveAll(r => r.Hash == CalcularHash(refreshToken)) > 0)
        {
            await GuardarAsync(usuario, lista);
        }
    }

    public async Task RevocarTodosLosRefreshTokensAsync(IdentityUser usuario)
    {
        await _userManager.RemoveAuthenticationTokenAsync(usuario, Proveedor, NombreRefreshTokens);
    }

    // ---------------------------------------------------------------------
    //  Internos
    // ---------------------------------------------------------------------

    private static string CrearRefreshToken(IdentityUser usuario) =>
        $"{usuario.Id}.{Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64))}";

    private static string CalcularHash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    /// <summary>El refresh token viaja como "&lt;userId&gt;.&lt;aleatorio&gt;" para localizar al usuario.</summary>
    private async Task<IdentityUser?> LocalizarUsuarioAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        var punto = refreshToken.IndexOf('.');
        if (punto <= 0 || punto == refreshToken.Length - 1) return null;

        return await _userManager.FindByIdAsync(refreshToken[..punto]);
    }

    private async Task<List<RegistroRefresh>> LeerAsync(IdentityUser usuario)
    {
        var valor = await _userManager.GetAuthenticationTokenAsync(
            usuario, Proveedor, NombreRefreshTokens);

        if (string.IsNullOrWhiteSpace(valor)) return new List<RegistroRefresh>();

        try
        {
            return JsonSerializer.Deserialize<List<RegistroRefresh>>(valor)
                   ?? new List<RegistroRefresh>();
        }
        catch (JsonException)
        {
            // Valor corrupto: se trata como si no hubiera sesiones.
            return new List<RegistroRefresh>();
        }
    }

    private async Task GuardarAsync(IdentityUser usuario, List<RegistroRefresh> lista)
    {
        if (lista.Count == 0)
        {
            await _userManager.RemoveAuthenticationTokenAsync(usuario, Proveedor, NombreRefreshTokens);
            return;
        }

        await _userManager.SetAuthenticationTokenAsync(
            usuario, Proveedor, NombreRefreshTokens, JsonSerializer.Serialize(lista));
    }

    private RegistroRefresh NuevoRegistro(string token) => new()
    {
        Hash = CalcularHash(token),
        CreadoEn = DateTimeOffset.UtcNow,
        ExpiraEn = DateTimeOffset.UtcNow.AddDays(_opciones.RefreshTokenDays),
    };

    private static List<RegistroRefresh> Podar(List<RegistroRefresh> lista)
    {
        var ahora = DateTimeOffset.UtcNow;
        return lista
            .Where(r => r.ExpiraEn > ahora)
            .OrderByDescending(r => r.CreadoEn)
            .Take(MaximoRefreshTokens)
            .ToList();
    }

    /// <summary>Registro persistido: solo el hash del refresh token, nunca el token.</summary>
    private sealed class RegistroRefresh
    {
        public string Hash { get; set; } = string.Empty;
        public DateTimeOffset CreadoEn { get; set; }
        public DateTimeOffset ExpiraEn { get; set; }
    }
}
