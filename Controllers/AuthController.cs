using System.Security.Claims;
using ApiPrimera.Interfaces;
using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ApiPrimera.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IJwtTokenService _tokens;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IJwtTokenService tokens)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokens = tokens;
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegistroViewModel input)
    {
        var email = input.Email.Trim().ToLowerInvariant();

        var existente = await _userManager.FindByEmailAsync(email);
        if (existente is not null)
        {
            return Conflict(new { exito = false, mensaje = "Ya existe una cuenta registrada con ese correo." });
        }

        var usuario = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var resultado = await _userManager.CreateAsync(usuario, input.Password);
        if (!resultado.Succeeded)
        {
            return BadRequest(new
            {
                exito = false,
                mensaje = "No se pudo crear la cuenta.",
                errores = resultado.Errors.Select(e => e.Description).ToArray()
            });
        }

        return Ok(new { exito = true, mensaje = "Cuenta creada correctamente. Ya puedes iniciar sesión." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel input)
    {
        var email = input.Email.Trim().ToLowerInvariant();

        var usuario = await _userManager.FindByEmailAsync(email);
        if (usuario is null)
        {
            return Unauthorized(new { exito = false, mensaje = "Correo o contraseña incorrectos." });
        }

        // PasswordSignInAsync valida y, si es correcto, emite la cookie de sesión
        // (la usan Razor Pages y el frontend estático con credentials: include).
        var resultado = await _signInManager.PasswordSignInAsync(
            usuario, input.Password, input.Recordarme, lockoutOnFailure: true);

        if (!resultado.Succeeded)
        {
            var mensaje = resultado.IsLockedOut
                ? "La cuenta está bloqueada temporalmente por intentos fallidos. Intenta más tarde."
                : "Correo o contraseña incorrectos.";
            return Unauthorized(new { exito = false, mensaje });
        }

        // Además de la cookie, se emite el par JWT:
        // access token de 15 minutos + refresh token rotativo de 7 días.
        var (accessToken, expiraEn) = _tokens.CrearAccessToken(usuario);
        var refreshToken = await _tokens.EmitirRefreshTokenAsync(usuario);

        return Ok(new
        {
            exito = true,
            mensaje = "Sesión iniciada.",
            email = usuario.Email,
            tokenType = "Bearer",
            accessToken,
            expiresIn = (int)(expiraEn - DateTimeOffset.UtcNow).TotalSeconds,
            refreshToken,
        });
    }

    /// <summary>
    /// Intercambia un refresh token válido por un nuevo par de tokens.
    /// El refresh token anterior queda consumido (rotación de uso único).
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshViewModel input)
    {
        var rotado = await _tokens.RotarRefreshTokenAsync(input.RefreshToken.Trim());
        if (rotado is null)
        {
            return Unauthorized(new
            {
                exito = false,
                mensaje = "Refresh token inválido o expirado. Inicia sesión de nuevo.",
            });
        }

        var (usuario, refreshToken) = rotado.Value;
        var (accessToken, expiraEn) = _tokens.CrearAccessToken(usuario);

        return Ok(new
        {
            exito = true,
            tokenType = "Bearer",
            accessToken,
            expiresIn = (int)(expiraEn - DateTimeOffset.UtcNow).TotalSeconds,
            refreshToken,
        });
    }

    [HttpGet("sesion")]
    public IActionResult Sesion()
    {
        if (User?.Identity?.IsAuthenticated == true)
        {
            return Ok(new { autenticado = true, email = User.Identity!.Name });
        }

        return Unauthorized(new { autenticado = false });
    }

    /// <summary>Ejemplo de endpoint protegido SOLO con Bearer JWT (401 sin token).</summary>
    [HttpGet("perfil")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult Perfil()
    {
        return Ok(new
        {
            autenticado = true,
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            email = User.Identity?.Name
                    ?? User.FindFirst("email")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value,
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutViewModel? input)
    {
        // Si llega un refresh token se revoca esa sesión JWT puntual;
        // si solo hay cookie de sesión se revocan todas las sesiones JWT.
        if (!string.IsNullOrWhiteSpace(input?.RefreshToken))
        {
            await _tokens.RevocarRefreshTokenAsync(input.RefreshToken);
        }
        else if (User?.Identity?.IsAuthenticated == true)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario is not null)
            {
                await _tokens.RevocarTodosLosRefreshTokensAsync(usuario);
            }
        }

        await _signInManager.SignOutAsync();
        return Ok(new { exito = true, mensaje = "Sesión cerrada." });
    }
}
