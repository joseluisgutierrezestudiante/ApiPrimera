using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class RegistroModel : PageModel
{
    [BindProperty]
    public RegistroViewModel Input { get; set; } = new();

    public string? Aviso { get; private set; }

    public void OnGet()
    {
        Aviso = "Vista de muestra: el registro de usuarios se habilitara en una entrega posterior.";
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        Aviso = "El formulario es visual por ahora: la creacion de cuentas se implementara mas adelante.";
        return Page();
    }
}