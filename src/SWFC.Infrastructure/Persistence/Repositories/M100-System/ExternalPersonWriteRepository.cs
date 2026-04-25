using Microsoft.EntityFrameworkCore;
using SWFC.Application.M100_System.M102_Organization.Users.ExternalPersons;
using SWFC.Domain.M100_System.M102_Organization.Users.ExternalPersons;
using SWFC.Infrastructure.Persistence.Context;

namespace SWFC.Infrastructure.Persistence.Repositories.M100_System;

public sealed class ExternalPersonWriteRepository : IExternalPersonWriteRepository
{
    private readonly AppDbContext _dbContext;

    public ExternalPersonWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ExternalPerson externalPerson,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(externalPerson);

        await _dbContext.Set<ExternalPerson>().AddAsync(externalPerson, cancellationToken);
    }

    public async Task<ExternalPerson?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ExternalPerson>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
