using System.ComponentModel.DataAnnotations;

namespace SWFC.Web.Models.Auth;

public sealed class ChangePasswordModel
{
    [Required(ErrorMessage = "Aktuelles Passwort ist erforderlich.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Neues Passwort ist erforderlich.")]
    [MinLength(12, ErrorMessage = "Das neue Passwort muss mindestens 12 Zeichen lang sein.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte neues Passwort bestätigen.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Die neue Passwortbestätigung stimmt nicht überein.")]
    [DataType(DataType.Password)]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
