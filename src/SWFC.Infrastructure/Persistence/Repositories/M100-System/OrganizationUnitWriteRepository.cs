using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Domain.M100_System.M102_Organization.Entities;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class OrganizationUnitWriteRepository : IOrganizationUnitWriteRepository
{
    private readonly AppDbContext _dbContext;

    public OrganizationUnitWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OrganizationUnit organizationUnit, CancellationToken cancellationToken = default)
    {
        await _dbContext.OrganizationUnits.AddAsync(organizationUnit, cancellationToken);
    }

    public Task<OrganizationUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.OrganizationUnits.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<OrganizationUnit?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var organizationUnit = _dbContext.OrganizationUnits
            .AsEnumerable()
            .FirstOrDefault(x => string.Equals(x.Code.Value, code, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(organizationUnit);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}