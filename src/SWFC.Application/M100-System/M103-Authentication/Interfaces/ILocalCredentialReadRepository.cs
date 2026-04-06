using SWFC.Application.M100_System.M103_Authentication.DTOs;

namespace SWFC.Application.M100_System.M103_Authentication.Interfaces;

public interface ILocalCredentialReadRepository
{
    Task<LocalCredentialRecordDto?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}