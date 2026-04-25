using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M100_System.M102_Organization.ShiftModels;

public sealed record GetShiftModelsQuery;

public sealed class GetShiftModelsPolicy : IAuthorizationPolicy<GetShiftModelsQuery>
{
    public AuthorizationRequirement GetRequirement(GetShiftModelsQuery request) =>
        new(requiredPermissions: new[] { "organization.read" });
}

public sealed class GetShiftModelsHandler : IUseCaseHandler<GetShiftModelsQuery, IReadOnlyList<ShiftModelListItem>>
{
    private readonly IShiftModelReadRepository _shiftModelReadRepository;

    public GetShiftModelsHandler(IShiftModelReadRepository shiftModelReadRepository)
    {
        _shiftModelReadRepository = shiftModelReadRepository;
    }

    public async Task<Result<IReadOnlyList<ShiftModelListItem>>> HandleAsync(
        GetShiftModelsQuery command,
        CancellationToken cancellationToken = default)
    {
        var shiftModels = await _shiftModelReadRepository.GetAllAsync(cancellationToken);

        var items = shiftModels
            .Select(x => new ShiftModelListItem(
                x.Id,
                x.Name.Value,
                x.Code.Value,
                x.Description,
                x.IsActive))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Result<IReadOnlyList<ShiftModelListItem>>.Success(items);
    }
}
