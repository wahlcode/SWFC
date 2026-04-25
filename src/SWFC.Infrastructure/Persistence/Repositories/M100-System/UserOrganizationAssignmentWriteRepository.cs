using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Assignments;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Assignments;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class UserOrganizationAssignmentWriteRepository : IUserOrganizationAssignmentWriteRepository
{
    private readonly AppDbContext _context;

    public UserOrganizationAssignmentWriteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AssignOrganizationUnitAsync(
        Guid userId,
        Guid organizationUnitId,
        bool isPrimary,
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

        var organizationUnitExists = await _context.OrganizationUnits
            .AsNoTracking()
            .AnyAsync(x => x.Id == organizationUnitId, cancellationToken);

        if (!organizationUnitExists)
        {
            return false;
        }

        var existingAssignments = await _context.UserOrganizationUnits
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        var activeAssignments = existingAssignments
            .Where(x => x.IsActive)
            .ToList();

        var shouldBePrimary = isPrimary || activeAssignments.Count == 0;

        var existingAssignment = existingAssignments
            .FirstOrDefault(x => x.OrganizationUnitId == organizationUnitId);

        if (existingAssignment is not null)
        {
            if (existingAssignment.IsActive)
            {
                if (shouldBePrimary && !existingAssignment.IsPrimary)
                {
                    foreach (var assignment in existingAssignments.Where(x => x.IsActive && x.IsPrimary))
                    {
                        assignment.SetPrimary(false, changeContext);
                    }

                    existingAssignment.SetPrimary(true, changeContext);
                    await _context.SaveChangesAsync(cancellationToken);
                    return true;
                }

                return false;
            }

            if (shouldBePrimary)
            {
                foreach (var assignment in existingAssignments.Where(x => x.IsActive && x.IsPrimary))
                {
                    assignment.SetPrimary(false, changeContext);
                }
            }

            existingAssignment.Activate(shouldBePrimary, changeContext);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        if (shouldBePrimary)
        {
            foreach (var assignment in existingAssignments.Where(x => x.IsActive && x.IsPrimary))
            {
                assignment.SetPrimary(false, changeContext);
            }
        }

        var userOrganizationUnit = UserOrganizationUnit.Create(
            userId,
            organizationUnitId,
            shouldBePrimary,
            changeContext);

        await _context.UserOrganizationUnits.AddAsync(userOrganizationUnit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RemoveOrganizationUnitAsync(
        Guid userId,
        Guid organizationUnitId,
        ChangeContext changeContext,
        CancellationToken cancellationToken = default)
    {
        var existingAssignments = await _context.UserOrganizationUnits
            .Where(x => x.UserId == userId && x.IsActive)
            .ToListAsync(cancellationToken);

        var assignmentsToRemove = existingAssignments
            .Where(x => x.OrganizationUnitId == organizationUnitId)
            .ToList();

        if (assignmentsToRemove.Count == 0)
        {
            return false;
        }

        if (existingAssignments.Count <= 1)
        {
            return false;
        }

        var removedPrimary = assignmentsToRemove.Any(x => x.IsPrimary);

        foreach (var assignment in assignmentsToRemove)
        {
            assignment.Deactivate(changeContext);
        }

        if (removedPrimary)
        {
            var replacement = existingAssignments
                .Where(x => x.IsActive && x.OrganizationUnitId != organizationUnitId)
                .OrderBy(x => x.AuditInfo.CreatedAtUtc)
                .FirstOrDefault();

            replacement?.SetPrimary(true, changeContext);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
