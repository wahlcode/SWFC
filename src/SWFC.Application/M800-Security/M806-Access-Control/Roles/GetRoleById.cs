using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M806_AccessControl.Roles;

public sealed record GetRoleByIdQuery(Guid Id);

public sealed class GetRoleByIdValidator : ICommandValidator<GetRoleByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetRoleByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Role id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetRoleByIdPolicy : IAuthorizationPolicy<GetRoleByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetRoleByIdQuery request) =>
        new(requiredPermissions: new[] { "security.read" });
}

public sealed class GetRoleByIdHandler : IUseCaseHandler<GetRoleByIdQuery, RoleDetailsDto>
{
    private readonly IRoleReadRepository _roleReadRepository;

    public GetRoleByIdHandler(IRoleReadRepository roleReadRepository)
    {
        _roleReadRepository = roleReadRepository;
    }

    public async Task<Result<RoleDetailsDto>> HandleAsync(
        GetRoleByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (role is null)
        {
            return Result<RoleDetailsDto>.Failure(
                new Error(
                    "m806.role.not_found",
                    "Role was not found.",
                    ErrorCategory.NotFound));
        }

        var dto = new RoleDetailsDto(
            role.Id,
            role.Name.Value,
            role.Description,
            role.IsActive,
            role.IsSystemRole);

        return Result<RoleDetailsDto>.Success(dto);
    }
}
