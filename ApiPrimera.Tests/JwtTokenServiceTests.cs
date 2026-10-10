using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ApiPrimera.Configuration;
using ApiPrimera.DB;
using ApiPrimera.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ApiPrimera.Tests;

/// <summary>
/// Pruebas del servicio JWT: access token de 15 minutos firmado con la clave
/// del entorno y refresh tokens con rotación de uso único.
/// </summary>
public class JwtTokenServiceTests : IDisposable
{
    private const string Clave = "clave-secreta-de-prueba-suficientemente-larga-123456";
    private const string Issuer = "ApiPrimera";
    private const string Audience = "AutoPrime";

    private readonly ContextoPrueba _ctx = new();
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtTokenService _servicio;

    public JwtTokenServiceTests()
    {
        var store = new UserStore<IdentityUser, IdentityRole, AppDbContext>(_ctx.Contexto);

        _userManager = new UserManager<IdentityUser>(
            store,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<IdentityUser>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            NullLogger<UserManager<IdentityUser>>.Instance);

        _servicio = new JwtTokenService(
            Options.Create(new JwtSettings
            {
                SecretKey = Clave,
                Issuer = Issuer,
                Audience = Audience,
                ExpirationMinutes = 15,
                RefreshTokenDays = 7,
            }),
            _userManager);
    }

    private TokenValidationParameters Parametros(string? clave = null) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = Issuer,
        ValidateAudience = true,
        ValidAudience = Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave ?? Clave)),
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.Zero,
    };

    private async Task<IdentityUser> CrearUsuarioAsync(string email = "ana@demo.com")
    {
        var usuario = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var resultado = await _userManager.CreateAsync(usuario, "Aa123456!");
        Assert.True(
            resultado.Succeeded,
            string.Join(" ", resultado.Errors.Select(e => e.Description)));

        return usuario;
    }

    [Fact]
    public void CrearAccessToken_firma_el_jwt_y_expira_a_los_15_minutos()
    {
        var usuario = new IdentityUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "ana@demo.com",
        };

        var (token, expiraEn) = _servicio.CrearAccessToken(usuario);

        // 1) La firma y la vida útil se validan con la clave del entorno.
        var principal = new JwtSecurityTokenHandler().ValidateToken(token, Parametros(), out _);
        Assert.NotNull(principal.Identity);
        Assert.True(principal.Identity!.IsAuthenticated);

        // 2) Ventana de vida exacta: 15 minutos (exp/iat van en segundos Unix).
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal(15d, (jwt.ValidTo - jwt.ValidFrom).TotalMinutes);
        Assert.True(
            (expiraEn.UtcDateTime - jwt.ValidTo).Duration() < TimeSpan.FromSeconds(1),
            $"exp del JWT ({jwt.ValidTo:O}) debe coincidir con lo devuelto ({expiraEn:O})");
        Assert.Equal(Issuer, jwt.Issuer);
        Assert.Equal(Audience, jwt.Audiences.Single());

        // 3) Reclamos útiles para la API.
        Assert.Contains(jwt.Claims, c => c.Value == "ana@demo.com");
        Assert.Contains(jwt.Claims, c => c.Type == "jti");
    }

    [Fact]
    public void CrearAccessToken_rechaza_firmas_realizadas_con_otra_clave()
    {
        var usuario = new IdentityUser { Id = Guid.NewGuid().ToString(), Email = "ana@demo.com" };
        var (token, _) = _servicio.CrearAccessToken(usuario);

        Assert.ThrowsAny<SecurityTokenException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(token, Parametros("otra-clave-totalmente-distinta-123"), out _));
    }

    [Fact]
    public async Task RotarRefreshToken_consume_el_token_y_emite_otro_nuevo()
    {
        var usuario = await CrearUsuarioAsync();
        var primero = await _servicio.EmitirRefreshTokenAsync(usuario);

        var rotado = await _servicio.RotarRefreshTokenAsync(primero);

        Assert.NotNull(rotado);
        Assert.Equal(usuario.Id, rotado!.Value.Usuario.Id);
        Assert.NotEqual(primero, rotado.Value.RefreshToken);

        // Uso único: reutilizar el token viejo ya no funciona.
        Assert.Null(await _servicio.RotarRefreshTokenAsync(primero));

        // El token recién emitido sí sirve.
        Assert.NotNull(await _servicio.RotarRefreshTokenAsync(rotado.Value.RefreshToken));
    }

    [Fact]
    public async Task RotarRefreshToken_rechaza_tokens_basura_o_de_usuarios_inexistentes()
    {
        Assert.Null(await _servicio.RotarRefreshTokenAsync("basura"));
        Assert.Null(await _servicio.RotarRefreshTokenAsync("con.punto-pero-sin-usuario"));
        Assert.Null(await _servicio.RotarRefreshTokenAsync($"{Guid.NewGuid()}.abc123"));
        Assert.Null(await _servicio.RotarRefreshTokenAsync(string.Empty));
    }

    [Fact]
    public async Task RevocarRefreshToken_especifico_no_afecta_a_otra_sesion()
    {
        var usuario = await CrearUsuarioAsync();
        var sesionCasa = await _servicio.EmitirRefreshTokenAsync(usuario);
        var sesionTrabajo = await _servicio.EmitirRefreshTokenAsync(usuario);

        await _servicio.RevocarRefreshTokenAsync(sesionCasa);

        Assert.Null(await _servicio.RotarRefreshTokenAsync(sesionCasa));
        Assert.NotNull(await _servicio.RotarRefreshTokenAsync(sesionTrabajo));
    }

    [Fact]
    public async Task RevocarTodosLosRefreshTokens_invalida_lo_que_haya_emitido()
    {
        var usuario = await CrearUsuarioAsync();
        var uno = await _servicio.EmitirRefreshTokenAsync(usuario);
        var dos = await _servicio.EmitirRefreshTokenAsync(usuario);

        await _servicio.RevocarTodosLosRefreshTokensAsync(usuario);

        Assert.Null(await _servicio.RotarRefreshTokenAsync(uno));
        Assert.Null(await _servicio.RotarRefreshTokenAsync(dos));
    }

    public void Dispose()
    {
        _userManager.Dispose();
        _ctx.Dispose();
        GC.SuppressFinalize(this);
    }
}
