using SWFC.Application.M100_System.M102_Organization.DTOs;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

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
        var role = await _roleReadRepository.GetByIdAsync(command.RoleId, cancellationToken);

        if (role is null)
        {
            return Result<RoleDetailsDto>.Failure(
                new Error(
                    "m102.role.not_found",
                    "Role was not found.",
                    ErrorCategory.NotFound));
        }

        var dto = new RoleDetailsDto(
            role.Id,
            role.Name.Value,
            role.Description);

        return Result<RoleDetailsDto>.Success(dto);
    }
}