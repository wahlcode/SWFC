using Microsoft.EntityFrameworkCore;
using SWFC.Application.M800_Security.M806_AccessControl.Assignments;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M800_Security.M806_AccessControl.Assignments;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M800_Security;

public sealed class UserRoleAssignmentWriteRepository : IUserRoleAssignmentWriteRepository
{
    private readonly AppDbContext _context;

    public UserRoleAssignmentWriteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AssignRoleAsync(
        Guid userId,
        Guid roleId,
        ChangeContext changeContext,
        CancellationToken cancellationToken = default)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, cancellationToken);

        if (!userExists)
        {
            return false;
        }

        var roleExists = await _context.Roles
            .AsNoTracking()
            .AnyAsync(x => x.Id == roleId, cancellationToken);

        if (!roleExists)
        {
            return false;
        }

        var existing = await _context.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId, cancellationToken);

        if (existing is not null)
        {
            if (existing.IsActive)
            {
                return false;
            }

            existing.Activate(changeContext);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        var userRole = UserRole.Create(userId, roleId, changeContext);

        await _context.UserRoles.AddAsync(userRole, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RemoveRoleAsync(
        Guid userId,
        Guid roleId,
        ChangeContext changeContext,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.RoleId == roleId && x.IsActive, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        existing.Deactivate(changeContext);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
