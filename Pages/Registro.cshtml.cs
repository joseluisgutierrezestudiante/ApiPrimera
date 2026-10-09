using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

/// <summary>
/// Vista de registro de usuario. Por ahora solo construye la vista y navega:
/// todavia no valida contra la API ni emite JWT, tokens, guards,
/// interceptores ni sesiones.
/// </summary>
public class RegistroModel : PageModel
{
    [BindProperty]
    public RegistroViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        TempData["Mensaje"] = "Registro listo. El alta de usuarios se conectara en una proxima etapa.";
        return RedirectToPage("/Login");
    }
}
