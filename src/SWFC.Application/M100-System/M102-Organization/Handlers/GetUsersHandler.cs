using SWFC.Application.M100_System.M102_Organization.DTOs;
using SWFC.Application.M100_System.M102_Organization.Interfaces;
using SWFC.Application.M100_System.M102_Organization.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M100_System.M102_Organization.Handlers;

public sealed class GetUsersHandler : IUseCaseHandler<GetUsersQuery, IReadOnlyList<UserListItem>>
{
    private readonly IUserReadRepository _userReadRepository;

    public GetUsersHandler(IUserReadRepository userReadRepository)
    {
        _userReadRepository = userReadRepository;
    }

    public async Task<Result<IReadOnlyList<UserListItem>>> HandleAsync(
        GetUsersQuery command,
        CancellationToken cancellationToken = default)
    {
        var users = await _userReadRepository.GetAllAsync(cancellationToken);

        var items = users
            .Select(x => new UserListItem(
                x.Id,
                x.IdentityKey.Value,
                x.DisplayName.Value,
                x.IsActive))
            .ToList();

        return Result<IReadOnlyList<UserListItem>>.Success(items);
    }
}