using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Application.M100_System.M102_Organization.Assignments;

public interface IUserOrganizationAssignmentWriteRepository
{
    Task<bool> AssignOrganizationUnitAsync(
        Guid userId,
        Guid organizationUnitId,
        bool isPrimary,
        ChangeContext changeContext,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveOrganizationUnitAsync(
        Guid userId,
        Guid organizationUnitId,
        ChangeContext changeContext,
        CancellationToken cancellationToken = default);
}
