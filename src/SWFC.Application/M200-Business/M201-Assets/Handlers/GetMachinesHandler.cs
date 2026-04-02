using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M200_Business.M201_Assets.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M200_Business.M201_Assets.Handlers;

public sealed class GetMachinesHandler : IUseCaseHandler<GetMachinesQuery, IReadOnlyList<MachineListItem>>
{
    private readonly IMachineReadRepository _machineReadRepository;

    public GetMachinesHandler(IMachineReadRepository machineReadRepository)
    {
        _machineReadRepository = machineReadRepository;
    }

    public async Task<Result<IReadOnlyList<MachineListItem>>> HandleAsync(
        GetMachinesQuery command,
        CancellationToken cancellationToken = default)
    {
        var machines = await _machineReadRepository.GetAllAsync(cancellationToken);
        return Result<IReadOnlyList<MachineListItem>>.Success(machines);
    }
}