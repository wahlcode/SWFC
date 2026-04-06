using SWFC.Application.M100_System.M102_Organization.DTOs;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

public sealed class GetRoleByIdHandler : IUseCaseHandler<GetRoleByIdQuery, RoleDetailsDto>
{
    private readonly IRoleReadRepository _roleReadRepository;
    private readonly IRolePermissionMapper _rolePermissionMapper;

    public GetRoleByIdHandler(
        IRoleReadRepository roleReadRepository,
        IRolePermissionMapper rolePermissionMapper)
    {
        _roleReadRepository = roleReadRepository;
        _rolePermissionMapper = rolePermissionMapper;
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

        var permissions = _rolePermissionMapper.Map(new[] { role.Name.Value });

        var dto = new RoleDetailsDto(
            role.Id,
            role.Name.Value,
            role.Description,
            permissions);

        return Result<RoleDetailsDto>.Success(dto);
    }
}