using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M200_Business.M201_Assets.Queries;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.M200_Business.M201_Assets.Handlers;

public sealed class GetMachineByIdHandler : IUseCaseHandler<GetMachineByIdQuery, MachineDetailsDto>
{
    private readonly IMachineReadRepository _machineReadRepository;

    public GetMachineByIdHandler(IMachineReadRepository machineReadRepository)
    {
        _machineReadRepository = machineReadRepository;
    }

    public async Task<Result<MachineDetailsDto>> HandleAsync(
        GetMachineByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var machine = await _machineReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (machine is null)
        {
            return Result<MachineDetailsDto>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"Machine '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        return Result<MachineDetailsDto>.Success(machine);
    }
}