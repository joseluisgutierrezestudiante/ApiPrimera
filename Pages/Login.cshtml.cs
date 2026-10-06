using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _sesion;

    public LoginModel(SignInManager<IdentityUser> sesion)
    {
        _sesion = sesion;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public string? Aviso { get; private set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Home");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var resultado = await _sesion.PasswordSignInAsync(
            Input.Email, Input.Password, Input.Recordarme, lockoutOnFailure: true);

        if (resultado.Succeeded)
        {
            return RedirectToPage("/Home");
        }

        if (resultado.IsLockedOut)
        {
            Aviso = "La cuenta esta bloqueada por intentos fallidos. Intenta de nuevo en unos minutos.";
            return Page();
        }

        Aviso = null;
        ModelState.AddModelError(string.Empty, "El correo o la contrasena no son correctos.");
        return Page();
    }
}
