using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Application.M200_Business.M201_Assets.Handlers;

public sealed class CreateMachineHandler : IUseCaseHandler<CreateMachineCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineWriteRepository _machineWriteRepository;

    public CreateMachineHandler(
        ICurrentUserService currentUserService,
        IMachineWriteRepository machineWriteRepository)
    {
        _currentUserService = currentUserService;
        _machineWriteRepository = machineWriteRepository;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var machineName = MachineName.Create(command.Name);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        var machine = SWFC.Domain.M200_Business.M201_Assets.Entities.Machine.Create(machineName, changeContext);

        await _machineWriteRepository.AddAsync(machine, cancellationToken);
        await _machineWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(machine.Id);
    }
}
