using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M806_AccessControl.Permissions;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class PermissionReadRepository : IPermissionReadRepository
{
    private readonly AppDbContext _dbContext;

    public PermissionReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PermissionListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Code)
            .Select(x => new PermissionListItemDto(
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.Module,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}
