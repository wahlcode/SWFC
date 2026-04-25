using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M803_Visibility;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M201_Assets.Machines;

public sealed record GetVisibleMachinesQuery;

public sealed class GetVisibleMachinesPolicy : IAuthorizationPolicy<GetVisibleMachinesQuery>
{
    public AuthorizationRequirement GetRequirement(GetVisibleMachinesQuery request)
        => new(requiredPermissions: new[] { "machine.read" });
}

public sealed class GetVisibleMachinesHandler : IUseCaseHandler<GetVisibleMachinesQuery, IReadOnlyList<MachineListItem>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineReadRepository _machineReadRepository;
    private readonly IVisibilityResolver _visibilityResolver;

    public GetVisibleMachinesHandler(
        ICurrentUserService currentUserService,
        IMachineReadRepository machineReadRepository,
        IVisibilityResolver visibilityResolver)
    {
        _currentUserService = currentUserService;
        _machineReadRepository = machineReadRepository;
        _visibilityResolver = visibilityResolver;
    }

    public async Task<Result<IReadOnlyList<MachineListItem>>> HandleAsync(
        GetVisibleMachinesQuery request,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var visibilityContext = VisibilityContext.Create(
            securityContext.UserId,
            securityContext.Roles,
            securityContext.OrganizationUnitIds);

        var machines = await _machineReadRepository.GetAllAsync(cancellationToken);
        var visibleMachines = new List<MachineListItem>();

        foreach (var machine in machines)
        {
            var canView = await _visibilityResolver.CanViewMachine(
                visibilityContext,
                machine.Id,
                cancellationToken);

            if (canView)
            {
                visibleMachines.Add(machine);
            }
        }

        return Result<IReadOnlyList<MachineListItem>>.Success(visibleMachines);
    }
}
