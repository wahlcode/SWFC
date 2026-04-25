using System.Text.Json;
using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Application.M100_System.M102_Organization.Users;

internal static class UserAuditPayloads
{
    public static string SerializeSnapshot(User user) =>
        JsonSerializer.Serialize(new
        {
            user.Id,
            IdentityKey = user.IdentityKey.Value,
            Username = user.Username.Value,
            DisplayName = user.DisplayName.Value,
            user.FirstName,
            user.LastName,
            user.EmployeeNumber,
            user.BusinessEmail,
            user.BusinessPhone,
            user.Plant,
            user.Location,
            user.Team,
            user.CostCenterId,
            user.ShiftModelId,
            user.JobFunction,
            Status = user.Status.ToString(),
            UserType = user.UserType.ToString()
        });

    public static string SerializeCreated(
        User user,
        string? costCenterLabel,
        string? shiftModelLabel,
        Guid primaryOrganizationUnitId,
        string primaryOrganizationUnitName,
        string primaryOrganizationUnitCode) =>
        JsonSerializer.Serialize(new
        {
            ChangeType = "Created",
            user.Id,
            IdentityKey = user.IdentityKey.Value,
            Username = user.Username.Value,
            DisplayName = user.DisplayName.Value,
            user.FirstName,
            user.LastName,
            user.EmployeeNumber,
            user.BusinessEmail,
            user.BusinessPhone,
            user.Plant,
            user.Location,
            user.Team,
            user.CostCenterId,
            CostCenter = costCenterLabel,
            user.ShiftModelId,
            ShiftModel = shiftModelLabel,
            user.JobFunction,
            PrimaryOrganization = new
            {
                Id = primaryOrganizationUnitId,
                Name = primaryOrganizationUnitName,
                Code = primaryOrganizationUnitCode
            },
            Status = user.Status.ToString(),
            UserType = user.UserType.ToString()
        });

    public static string SerializeUpdated(
        User user,
        string? costCenterLabel,
        string? shiftModelLabel,
        IReadOnlyCollection<string> organizationUnits) =>
        JsonSerializer.Serialize(new
        {
            ChangeType = "Updated",
            user.Id,
            IdentityKey = user.IdentityKey.Value,
            Username = user.Username.Value,
            DisplayName = user.DisplayName.Value,
            user.FirstName,
            user.LastName,
            user.EmployeeNumber,
            user.BusinessEmail,
            user.BusinessPhone,
            user.Plant,
            user.Location,
            user.Team,
            user.CostCenterId,
            CostCenter = costCenterLabel,
            user.ShiftModelId,
            ShiftModel = shiftModelLabel,
            user.JobFunction,
            Status = user.Status.ToString(),
            UserType = user.UserType.ToString(),
            OrganizationUnits = organizationUnits
        });
}
