using SWFC.Application.M100_System.M102_Organization.DTOs;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

public sealed class GetRolesHandler : IUseCaseHandler<GetRolesQuery, IReadOnlyList<RoleListItem>>
{
    private readonly IRoleReadRepository _roleReadRepository;

    public GetRolesHandler(IRoleReadRepository roleReadRepository)
    {
        _roleReadRepository = roleReadRepository;
    }

    public async Task<Result<IReadOnlyList<RoleListItem>>> HandleAsync(
        GetRolesQuery command,
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleReadRepository.GetAllAsync(cancellationToken);

        var items = roles
            .Select(x => new RoleListItem(
                x.Id,
                x.Name.Value,
                x.Description))
            .ToList();

        return Result<IReadOnlyList<RoleListItem>>.Success(items);
    }
}