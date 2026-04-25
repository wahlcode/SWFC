using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Application.M100_System.M102_Organization.Users;

public static class UserHistorySnapshots
{
    public static string Create(
        User user,
        IReadOnlyCollection<string> organizationUnits)
    {
        var organizationUnitValue = organizationUnits.Count == 0
            ? "-"
            : string.Join("; ", organizationUnits);

        var costCenterValue = user.CostCenterId?.ToString() ?? "-";
        var shiftModelValue = user.ShiftModelId?.ToString() ?? "-";

        return string.Join(
            " | ",
            $"IdentityKey={user.IdentityKey.Value}",
            $"Username={user.Username.Value}",
            $"DisplayName={user.DisplayName.Value}",
            $"FirstName={user.FirstName}",
            $"LastName={user.LastName}",
            $"EmployeeNumber={user.EmployeeNumber}",
            $"BusinessEmail={user.BusinessEmail}",
            $"BusinessPhone={user.BusinessPhone}",
            $"Plant={user.Plant}",
            $"Location={user.Location}",
            $"Team={user.Team}",
            $"CostCenterId={costCenterValue}",
            $"ShiftModelId={shiftModelValue}",
            $"JobFunction={user.JobFunction}",
            $"Status={user.Status}",
            $"UserType={user.UserType}",
            $"IsActive={user.IsActive}",
            $"OrganizationUnits={organizationUnitValue}");
    }
}
