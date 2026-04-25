using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M803_Visibility;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M201_Assets.MachineComponents;

public sealed record GetVisibleMachineComponentsByMachineQuery(Guid MachineId);

public sealed class GetVisibleMachineComponentsByMachineValidator : ICommandValidator<GetVisibleMachineComponentsByMachineQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetVisibleMachineComponentsByMachineQuery command,
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

public sealed class GetVisibleMachineComponentsByMachinePolicy : IAuthorizationPolicy<GetVisibleMachineComponentsByMachineQuery>
{
    public AuthorizationRequirement GetRequirement(GetVisibleMachineComponentsByMachineQuery request)
        => new(requiredPermissions: new[] { "machine.read" });
}

public sealed class GetVisibleMachineComponentsByMachineHandler
    : IUseCaseHandler<GetVisibleMachineComponentsByMachineQuery, IReadOnlyList<MachineComponentListItemDto>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineComponentReadRepository _readRepository;
    private readonly IVisibilityResolver _visibilityResolver;

    public GetVisibleMachineComponentsByMachineHandler(
        ICurrentUserService currentUserService,
        IMachineComponentReadRepository readRepository,
        IVisibilityResolver visibilityResolver)
    {
        _currentUserService = currentUserService;
        _readRepository = readRepository;
        _visibilityResolver = visibilityResolver;
    }

    public async Task<Result<IReadOnlyList<MachineComponentListItemDto>>> HandleAsync(
        GetVisibleMachineComponentsByMachineQuery request,
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
            return Result<IReadOnlyList<MachineComponentListItemDto>>.Failure(new Error(
                GeneralErrorCodes.Unauthorized,
                "Machine is not visible for the current user.",
                ErrorCategory.Security));
        }

        var items = await _readRepository.GetByMachineAsync(request.MachineId, cancellationToken);

        var visibleItems = new List<MachineComponentListItemDto>();

        foreach (var item in items)
        {
            var canView = await _visibilityResolver.CanViewComponent(
                visibilityContext,
                item.Id,
                cancellationToken);

            if (canView)
            {
                visibleItems.Add(item);
            }
        }

        return Result<IReadOnlyList<MachineComponentListItemDto>>.Success(visibleItems);
    }
}
