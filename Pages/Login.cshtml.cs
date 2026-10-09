using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

/// <summary>
/// Vista de inicio de sesion. Por ahora solo construye la vista y navega:
/// todavia no valida contra la API ni emite JWT, tokens, guards,
/// interceptores ni sesiones.
/// </summary>
public class LoginModel : PageModel
{
    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        TempData["Mensaje"] = "Bienvenido. El inicio de sesion se conectara en una proxima etapa.";
        return RedirectToPage("/Home");
    }
}
