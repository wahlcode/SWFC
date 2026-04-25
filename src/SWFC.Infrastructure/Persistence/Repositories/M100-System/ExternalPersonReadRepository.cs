using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Users.ExternalPersons;
using SWFC.Domain.M100_System.M102_Organization.Users.ExternalPersons;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class ExternalPersonReadRepository : IExternalPersonReadRepository
{
    private readonly AppDbContext _dbContext;

    public ExternalPersonReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExternalPerson?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ExternalPerson>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ExternalPerson>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ExternalPerson>()
            .AsNoTracking()
            .OrderBy(x => x.DisplayName)
            .ThenBy(x => x.CompanyName)
            .ToListAsync(cancellationToken);
    }
}
