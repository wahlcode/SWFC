using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M806_AccessControl.RolePermissions;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class RolePermissionReadRepository : IRolePermissionReadRepository
{
    private readonly AppDbContext _dbContext;

    public RolePermissionReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RolePermissionListItemDto>> GetByRoleIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var assignedPermissionIds = await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == roleId && x.IsActive)
            .Select(x => x.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Code)
            .Select(x => new RolePermissionListItemDto(
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.Module,
                assignedPermissionIds.Contains(x.Id),
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}
