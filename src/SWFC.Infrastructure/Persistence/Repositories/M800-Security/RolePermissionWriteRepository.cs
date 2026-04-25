using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M806_AccessControl.RolePermissions;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M800_Security.M806_AccessControl.RolePermissions;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class RolePermissionWriteRepository : IRolePermissionWriteRepository
{
    private readonly AppDbContext _dbContext;

    public RolePermissionWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SetPermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        string changedByUserId,
        CancellationToken cancellationToken = default)
    {
        var requestedPermissionIds = permissionIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToHashSet();

        var existingAssignments = await _dbContext.RolePermissions
            .Where(x => x.RoleId == roleId)
            .ToListAsync(cancellationToken);

        var changeContext = ChangeContext.Create(
            changedByUserId,
            "Role permissions updated.");

        foreach (var assignment in existingAssignments)
        {
            if (requestedPermissionIds.Contains(assignment.PermissionId))
            {
                assignment.Activate(changeContext);
            }
            else
            {
                assignment.Deactivate(changeContext);
            }
        }

        var existingPermissionIds = existingAssignments
            .Select(x => x.PermissionId)
            .ToHashSet();

        var missingPermissionIds = requestedPermissionIds
            .Where(x => !existingPermissionIds.Contains(x))
            .ToList();

        foreach (var permissionId in missingPermissionIds)
        {
            var assignment = RolePermission.Create(
                roleId,
                permissionId,
                changeContext);

            await _dbContext.RolePermissions.AddAsync(assignment, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
