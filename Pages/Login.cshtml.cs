using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public string? Aviso { get; private set; }

    public void OnGet()
    {
        Aviso = "Vista de muestra: el inicio de sesion se habilitara en una entrega posterior.";
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Aviso = "El formulario es visual por ahora: la autenticacion se implementara mas adelante.";
        return Page();
    }
}