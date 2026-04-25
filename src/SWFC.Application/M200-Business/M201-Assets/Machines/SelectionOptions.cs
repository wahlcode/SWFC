using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M201_Assets.Machines;

public sealed record GetMachineSelectionOptionsQuery(Guid? ExcludeMachineId = null);

public sealed class GetMachineSelectionOptionsPolicy : IAuthorizationPolicy<GetMachineSelectionOptionsQuery>
{
    public AuthorizationRequirement GetRequirement(GetMachineSelectionOptionsQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "machine.read" });
    }
}

public sealed class GetMachineSelectionOptionsHandler : IUseCaseHandler<GetMachineSelectionOptionsQuery, IReadOnlyList<MachineSelectionOptionDto>>
{
    private readonly IMachineReadRepository _machineReadRepository;

    public GetMachineSelectionOptionsHandler(IMachineReadRepository machineReadRepository)
    {
        _machineReadRepository = machineReadRepository;
    }

    public async Task<Result<IReadOnlyList<MachineSelectionOptionDto>>> HandleAsync(
        GetMachineSelectionOptionsQuery command,
        CancellationToken cancellationToken = default)
    {
        var items = await _machineReadRepository.GetSelectionOptionsAsync(command.ExcludeMachineId, cancellationToken);
        return Result<IReadOnlyList<MachineSelectionOptionDto>>.Success(items);
    }
}

public sealed record GetOrganizationUnitSelectionOptionsQuery;

public sealed class GetOrganizationUnitSelectionOptionsPolicy : IAuthorizationPolicy<GetOrganizationUnitSelectionOptionsQuery>
{
    public AuthorizationRequirement GetRequirement(GetOrganizationUnitSelectionOptionsQuery request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "machine.read" });
    }
}

public sealed class GetOrganizationUnitSelectionOptionsHandler : IUseCaseHandler<GetOrganizationUnitSelectionOptionsQuery, IReadOnlyList<OrganizationUnitSelectionOptionDto>>
{
    private readonly IMachineReadRepository _machineReadRepository;

    public GetOrganizationUnitSelectionOptionsHandler(IMachineReadRepository machineReadRepository)
    {
        _machineReadRepository = machineReadRepository;
    }

    public async Task<Result<IReadOnlyList<OrganizationUnitSelectionOptionDto>>> HandleAsync(
        GetOrganizationUnitSelectionOptionsQuery command,
        CancellationToken cancellationToken = default)
    {
        var items = await _machineReadRepository.GetOrganizationUnitSelectionOptionsAsync(cancellationToken);
        return Result<IReadOnlyList<OrganizationUnitSelectionOptionDto>>.Success(items);
    }
}

