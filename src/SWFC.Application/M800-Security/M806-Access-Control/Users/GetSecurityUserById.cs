using SWFC.Application.M100_System.M102_Organization.Users;
using SWFC.Application.M100_System.M103_Authentication;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M806_AccessControl.Shared;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M806_AccessControl.Users;

public sealed record GetSecurityUserByIdQuery(Guid UserId);

public sealed class GetSecurityUserByIdValidator : ICommandValidator<GetSecurityUserByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetSecurityUserByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.UserId == Guid.Empty)
        {
            result.Add("UserId", "User id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetSecurityUserByIdPolicy : IAuthorizationPolicy<GetSecurityUserByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetSecurityUserByIdQuery request) =>
        new(requiredPermissions: new[] { "security.read" });
}

public sealed class GetSecurityUserByIdHandler : IUseCaseHandler<GetSecurityUserByIdQuery, SecurityUserDetailsDto>
{
    private readonly IUserReadRepository _userReadRepository;
    private readonly ISecurityAdministrationGuard _securityAdministrationGuard;
    private readonly ILocalAuthenticationService _localAuthenticationService;

    public GetSecurityUserByIdHandler(
        IUserReadRepository userReadRepository,
        ISecurityAdministrationGuard securityAdministrationGuard,
        ILocalAuthenticationService localAuthenticationService)
    {
        _userReadRepository = userReadRepository;
        _securityAdministrationGuard = securityAdministrationGuard;
        _localAuthenticationService = localAuthenticationService;
    }

    public async Task<Result<SecurityUserDetailsDto>> HandleAsync(
        GetSecurityUserByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userReadRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result<SecurityUserDetailsDto>.Failure(
                new Error(
                    "m806.security_user.not_found",
                    "User was not found.",
                    ErrorCategory.NotFound));
        }

        var roleNames = await _securityAdministrationGuard.GetActiveRoleNamesAsync(user.Id, cancellationToken);
        var credentialStatus = await _localAuthenticationService.GetStatusAsync(user.Id, cancellationToken);
        var isProtectedSuperAdmin = await _securityAdministrationGuard.IsProtectedSuperAdminAsync(user.Id, cancellationToken);

        return Result<SecurityUserDetailsDto>.Success(
            new SecurityUserDetailsDto(
                user.Id,
                user.IdentityKey.Value,
                user.Username.Value,
                user.DisplayName.Value,
                user.FirstName,
                user.LastName,
                user.EmployeeNumber,
                user.BusinessEmail,
                user.Status,
                user.UserType,
                roleNames.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray(),
                isProtectedSuperAdmin,
                credentialStatus));
    }
}
