using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Assignments;

namespace SWFC.Domain.M100_System.M102_Organization.Users;

public sealed partial class User
{
    public bool HasActiveOrganizationUnit(Guid organizationUnitId) =>
        _organizationUnits.Any(x => x.OrganizationUnitId == organizationUnitId && x.IsActive);

    public Guid? GetPrimaryOrganizationUnitId() =>
        _organizationUnits
            .Where(x => x.IsActive && x.IsPrimary)
            .Select(x => x.OrganizationUnitId)
            .Cast<Guid?>()
            .FirstOrDefault();

    public int GetActiveOrganizationUnitCount() =>
        _organizationUnits.Count(x => x.IsActive);

    public void AssignOrganizationUnit(
        Guid organizationUnitId,
        bool isPrimary,
        ChangeContext changeContext)
    {
        var existingAssignment = _organizationUnits
            .FirstOrDefault(x => x.OrganizationUnitId == organizationUnitId);

        if (existingAssignment is not null)
        {
            if (existingAssignment.IsActive)
            {
                if (isPrimary && !existingAssignment.IsPrimary)
                {
                    foreach (var item in _organizationUnits.Where(x => x.IsActive))
                    {
                        item.SetPrimary(item.OrganizationUnitId == organizationUnitId, changeContext);
                    }
                }

                return;
            }

            if (isPrimary || !_organizationUnits.Any(x => x.IsActive && x.IsPrimary))
            {
                foreach (var item in _organizationUnits.Where(x => x.IsActive && x.IsPrimary))
                {
                    item.SetPrimary(false, changeContext);
                }
            }

            existingAssignment.Activate(isPrimary, changeContext);
            return;
        }

        if (isPrimary)
        {
            foreach (var item in _organizationUnits.Where(x => x.IsActive))
            {
                item.SetPrimary(false, changeContext);
            }
        }

        _organizationUnits.Add(
            UserOrganizationUnit.Create(
                Id,
                organizationUnitId,
                isPrimary,
                changeContext));
    }

    public void RemoveOrganizationUnit(
        Guid organizationUnitId,
        ChangeContext changeContext)
    {
        var existing = _organizationUnits.FirstOrDefault(
            x => x.OrganizationUnitId == organizationUnitId && x.IsActive);

        if (existing is null)
        {
            return;
        }

        existing.Deactivate(changeContext);
    }
}
