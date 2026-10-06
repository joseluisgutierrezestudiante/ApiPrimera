using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<IdentityUser> _sesion;

    public LogoutModel(SignInManager<IdentityUser> sesion)
    {
        _sesion = sesion;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _sesion.SignOutAsync();
        return RedirectToPage("/Home");
    }
}
