using SWFC.Application.M100_System.M102_Organization.DTOs;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

public sealed class GetUserByIdHandler : IUseCaseHandler<GetUserByIdQuery, UserDetailsDto>
{
    private readonly IUserReadRepository _userReadRepository;

    public GetUserByIdHandler(IUserReadRepository userReadRepository)
    {
        _userReadRepository = userReadRepository;
    }

    public async Task<Result<UserDetailsDto>> HandleAsync(
        GetUserByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userReadRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return Result<UserDetailsDto>.Failure(
                new Error(
                    "m102.user.not_found",
                    "User was not found.",
                    ErrorCategory.NotFound));
        }

        var dto = new UserDetailsDto(
            user.Id,
            user.IdentityKey.Value,
            user.Username.Value,
            user.DisplayName.Value,
            user.IsActive,
            Array.Empty<string>(),
            Array.Empty<OrganizationUnitReference>());

        return Result<UserDetailsDto>.Success(dto);
    }
}