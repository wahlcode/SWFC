using System.ComponentModel.DataAnnotations;
using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Web.Pages.M100_System.M102_Organization.Users;

public sealed class CreateUserModel
{
    [Required(ErrorMessage = "Identity key is required.")]
    public string IdentityKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Display name is required.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Employee number is required.")]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Business email is required.")]
    [EmailAddress(ErrorMessage = "Business email is invalid.")]
    public string BusinessEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Business phone is required.")]
    public string BusinessPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Plant is required.")]
    public string Plant { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location is required.")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Team is required.")]
    public string Team { get; set; } = string.Empty;

    public Guid? CostCenterId { get; set; }

    public Guid? ShiftModelId { get; set; }

    [Required(ErrorMessage = "Job function is required.")]
    public string JobFunction { get; set; } = string.Empty;

    [Required(ErrorMessage = "Language is required.")]
    public string PreferredCultureName { get; set; } = "en-US";

    [Required(ErrorMessage = "Primary organization unit is required.")]
    public Guid? PrimaryOrganizationUnitId { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public UserType UserType { get; set; } = UserType.Internal;

    [Required(ErrorMessage = "Reason is required.")]
    public string Reason { get; set; } = string.Empty;
}
