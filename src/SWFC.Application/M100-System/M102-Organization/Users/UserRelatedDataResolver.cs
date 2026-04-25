using SWFC.Application.M100_System.M102_Organization.CostCenters;
using SWFC.Application.M100_System.M102_Organization.OrganizationUnits;
using SWFC.Application.M100_System.M102_Organization.ShiftModels;
using SWFC.Domain.M100_System.M102_Organization.Users;

namespace SWFC.Application.M100_System.M102_Organization.Users;

internal static class UserRelatedDataResolver
{
    public static async Task<string?> ResolveCostCenterLabelAsync(
        ICostCenterReadRepository costCenterReadRepository,
        Guid? costCenterId,
        CancellationToken cancellationToken)
    {
        if (!costCenterId.HasValue)
        {
            return null;
        }

        var costCenter = await costCenterReadRepository.GetByIdAsync(costCenterId.Value, cancellationToken);

        return costCenter is null
            ? null
            : $"{costCenter.Name.Value} ({costCenter.Code.Value})";
    }

    public static async Task<string?> ResolveShiftModelLabelAsync(
        IShiftModelReadRepository shiftModelReadRepository,
        Guid? shiftModelId,
        CancellationToken cancellationToken)
    {
        if (!shiftModelId.HasValue)
        {
            return null;
        }

        var shiftModel = await shiftModelReadRepository.GetByIdAsync(shiftModelId.Value, cancellationToken);

        return shiftModel is null
            ? null
            : $"{shiftModel.Name.Value} ({shiftModel.Code.Value})";
    }

    public static async Task<IReadOnlyCollection<string>> ResolveOrganizationUnitsAsync(
        IOrganizationUnitReadRepository organizationUnitReadRepository,
        User user,
        CancellationToken cancellationToken)
    {
        var items = new List<string>();

        foreach (var assignment in user.OrganizationUnits.Where(x => x.IsActive))
        {
            var organizationUnit = await organizationUnitReadRepository.GetByIdAsync(
                assignment.OrganizationUnitId,
                cancellationToken);

            if (organizationUnit is null)
            {
                continue;
            }

            items.Add($"{organizationUnit.Name.Value} ({organizationUnit.Code.Value}){(assignment.IsPrimary ? " [Primary]" : string.Empty)}");
        }

        return items;
    }
}
