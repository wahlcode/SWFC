using SWFC.Application.M100_System.M103_Authentication.DTOs;

namespace SWFC.Application.M100_System.M103_Authentication.Interfaces;

public interface ILocalAuthenticationService
{
    Task<AuthenticationResultDto> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    Task ChangeOwnPasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task AdminSetPasswordAsync(
        Guid userId,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<LocalCredentialStatusDto?> GetStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}