using SWFC.Application.M100_System.M103_Authentication.DTOs;

namespace SWFC.Application.M100_System.M103_Authentication.Interfaces;

public interface ILocalCredentialWriteRepository
{
    Task<LocalCredentialRecordDto?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        LocalCredentialRecordDto credential,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        LocalCredentialRecordDto credential,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}