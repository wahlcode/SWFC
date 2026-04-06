using System.ComponentModel.DataAnnotations;

namespace SWFC.Web.Models.Auth;

public sealed class LoginModel
{
    [Required(ErrorMessage = "Benutzername ist erforderlich.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Benutzername muss zwischen 3 und 100 Zeichen lang sein.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passwort ist erforderlich.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}