using System.ComponentModel.DataAnnotations;

namespace ApiPrimera.Models.ViewModels;

public class RefreshViewModel
{
    [Required(ErrorMessage = "El refresh token es obligatorio.")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>Body opcional del logout: si llega el refresh token se revoca esa sesión.</summary>
public class LogoutViewModel
{
    public string? RefreshToken { get; set; }
}
