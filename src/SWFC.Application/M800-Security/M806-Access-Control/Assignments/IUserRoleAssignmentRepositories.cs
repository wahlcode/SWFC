using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Application.M800_Security.M806_AccessControl.Assignments;

public interface IUserRoleAssignmentWriteRepository
{
    Task<bool> AssignRoleAsync(
        Guid userId,
        Guid roleId,
        ChangeContext changeContext,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveRoleAsync(
        Guid userId,
        Guid roleId,
        ChangeContext changeContext,
        CancellationToken cancellationToken = default);
}
