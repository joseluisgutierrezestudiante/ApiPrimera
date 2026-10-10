using ApiPrimera.Models.ViewModels;
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

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

        // PasswordSignInAsync valida y, si es correcto, emite la cookie de sesión.
        var resultado = await _signInManager.PasswordSignInAsync(
            usuario, input.Password, input.Recordarme, lockoutOnFailure: true);

        if (!resultado.Succeeded)
        {
            var mensaje = resultado.IsLockedOut
                ? "La cuenta está bloqueada temporalmente por intentos fallidos. Intenta más tarde."
                : "Correo o contraseña incorrectos.";
            return Unauthorized(new { exito = false, mensaje });
        }

        return Ok(new { exito = true, mensaje = "Sesión iniciada.", email = usuario.Email });
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

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { exito = true, mensaje = "Sesión cerrada." });
    }
}
