using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class LoginModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost(string email, string password)
    {
        // Por ahora, solo redirige. No hay autenticación real.
        return RedirectToPage("/home");
    }
}
