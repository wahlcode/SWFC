using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M803_Visibility;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M201_Assets.MachineComponentAreas;

public sealed record GetVisibleMachineComponentAreasQuery(Guid MachineId);

public sealed class GetVisibleMachineComponentAreasValidator : ICommandValidator<GetVisibleMachineComponentAreasQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetVisibleMachineComponentAreasQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.MachineId == Guid.Empty)
        {
            result.Add("MachineId", "Machine id is invalid.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetVisibleMachineComponentAreasPolicy : IAuthorizationPolicy<GetVisibleMachineComponentAreasQuery>
{
    public AuthorizationRequirement GetRequirement(GetVisibleMachineComponentAreasQuery request)
        => new(requiredPermissions: new[] { "machine.read" });
}

public sealed class GetVisibleMachineComponentAreasHandler
    : IUseCaseHandler<GetVisibleMachineComponentAreasQuery, IReadOnlyList<MachineComponentAreaListItemDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineComponentAreaReadRepository _readRepository;
    private readonly IVisibilityResolver _visibilityResolver;

    public GetVisibleMachineComponentAreasHandler(
        ICurrentUserService currentUserService,
        IMachineComponentAreaReadRepository readRepository,
        IVisibilityResolver visibilityResolver)
    {
        _currentUserService = currentUserService;
        _readRepository = readRepository;
        _visibilityResolver = visibilityResolver;
    }

    public async Task<Result<IReadOnlyList<MachineComponentAreaListItemDto>>> HandleAsync(
        GetVisibleMachineComponentAreasQuery request,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var visibilityContext = VisibilityContext.Create(
            securityContext.UserId,
            securityContext.Roles,
            securityContext.OrganizationUnitIds);

        var machineVisible = await _visibilityResolver.CanViewMachine(
            visibilityContext,
            request.MachineId,
            cancellationToken);

        if (!machineVisible)
        {
            return Result<IReadOnlyList<MachineComponentAreaListItemDto>>.Success(Array.Empty<MachineComponentAreaListItemDto>());
        }

        var items = await _readRepository.GetAllAsync(cancellationToken);

        var visibleItems = new List<MachineComponentAreaListItemDto>();

        foreach (var item in items)
        {
            var canViewArea = await _visibilityResolver.CanViewArea(
                visibilityContext,
                item.Id,
                cancellationToken);

            if (canViewArea)
            {
                visibleItems.Add(item);
            }
        }

        return Result<IReadOnlyList<MachineComponentAreaListItemDto>>.Success(visibleItems);
    }
}
