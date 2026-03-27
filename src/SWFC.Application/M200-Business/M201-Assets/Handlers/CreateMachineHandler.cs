using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M200_Business.M201_Assets.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Application.M200_Business.M201_Assets.Handlers;

public sealed class CreateMachineHandler
    : AuthorizedHandler<CreateMachineCommand, Guid>,
      IUseCaseHandler<CreateMachineCommand, Guid>
{
    private readonly ICommandValidator<CreateMachineCommand> _validator;
    private readonly IMachineWriteRepository _machineWriteRepository;

    public CreateMachineHandler(
        ICommandValidator<CreateMachineCommand> validator,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IAuthorizationPolicy<CreateMachineCommand> authorizationPolicy,
        IMachineWriteRepository machineWriteRepository)
        : base(currentUserService, authorizationService, authorizationPolicy)
    {
        _validator = validator;
        _machineWriteRepository = machineWriteRepository;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateMachineCommand command,
        CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);

        if (!validation.IsValid)
        {
            var message = string.Join("; ", validation.Errors.Select(x => x.Message));

            return Result<Guid>.Failure(
                new Error(
                    ErrorCodes.General.ValidationFailed,
                    message,
                    ErrorCategory.Validation));
        }

        return await AuthorizeAndHandleAsync(command, cancellationToken);
    }

    protected override async Task<Result<Guid>> HandleAuthorizedCoreAsync(
        CreateMachineCommand command,
        SecurityContext securityContext,
        CancellationToken cancellationToken)
    {
        var machineName = MachineName.Create(command.Name);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        var machine = SWFC.Domain.M200_Business.M201_Assets.Entities.Machine.Create(machineName, changeContext);

        await _machineWriteRepository.AddAsync(machine, cancellationToken);
        await _machineWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(machine.Id);
    }
}