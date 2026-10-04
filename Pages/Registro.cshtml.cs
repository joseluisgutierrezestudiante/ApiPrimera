using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class RegistroModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost(string nombre, string email, string password, string confirmPassword)
    {
        // Por ahora, solo redirige. No hay registro real en este taller.
        if (password != confirmPassword)
        {
            ModelState.AddModelError("", "Las contraseñas no coinciden.");
            return Page();
        }

        return RedirectToPage("/login");
    }
}
