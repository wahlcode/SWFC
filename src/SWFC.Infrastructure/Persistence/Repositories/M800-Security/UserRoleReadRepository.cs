using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M806_AccessControl.Assignments;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class UserRoleReadRepository : IUserRoleReadRepository
{
    private readonly AppDbContext _dbContext;

    public UserRoleReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Guid>> GetActiveRoleIdsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive)
            .Select(x => x.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> GetActiveRoleNamesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserRoles
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive)
            .Join(
                _dbContext.Roles.AsNoTracking().Where(x => x.IsActive),
                userRole => userRole.RoleId,
                role => role.Id,
                (_, role) => role.Name.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
