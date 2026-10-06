using System.Security.Claims;
using ApiPrimera.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ApiPrimera.Pages;

public class RegistroModel : PageModel
{
    private readonly UserManager<IdentityUser> _usuarios;
    private readonly SignInManager<IdentityUser> _sesion;

    public RegistroModel(UserManager<IdentityUser> usuarios, SignInManager<IdentityUser> sesion)
    {
        _usuarios = usuarios;
        _sesion = sesion;
    }

    [BindProperty]
    public RegistroViewModel Input { get; set; } = new();

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

        var usuario = new IdentityUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true
        };

        var resultado = await _usuarios.CreateAsync(usuario, Input.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, MensajeDe(error));
            }

            return Page();
        }

        await _usuarios.AddClaimAsync(usuario, new Claim("nombre", Input.Nombre));
        await _sesion.SignInAsync(usuario, isPersistent: false);

        TempData["Mensaje"] = $"Cuenta creada. Bienvenido, {Input.Nombre}.";
        return RedirectToPage("/Home");
    }

    private static string MensajeDe(IdentityError error) => error.Code switch
    {
        "PasswordTooShort" => "La contrasena debe tener al menos 8 caracteres.",
        "PasswordRequiresDigit" => "La contrasena debe incluir al menos un numero.",
        "PasswordRequiresUpper" => "La contrasena debe incluir al menos una letra mayuscula.",
        "PasswordRequiresLower" => "La contrasena debe incluir al menos una letra minuscula.",
        "PasswordRequiresNonAlphanumeric" => "La contrasena debe incluir un caracter especial.",
        "InvalidEmail" => "El formato del correo electronico no es valido.",
        "DuplicateUserName" or "DuplicateEmail" => "Ese correo electronico ya esta registrado.",
        "UserAlreadyHasPassword" => "La cuenta ya tiene contrasena.",
        _ => error.Description
    };
}
