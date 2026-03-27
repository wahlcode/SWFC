using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M201_Assets.Commands;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.Entities;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Application.M200_Business.M201_Assets.Handlers;

public sealed class CreateMachineHandler
    : AuthorizedHandler<CreateMachineCommand, Guid>,
      IUseCaseHandler<CreateMachineCommand, Guid>
{
    private readonly ICommandValidator<CreateMachineCommand> _validator;

    public CreateMachineHandler(
        ICommandValidator<CreateMachineCommand> validator,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IAuthorizationPolicy<CreateMachineCommand> authorizationPolicy)
        : base(currentUserService, authorizationService, authorizationPolicy)
    {
        _validator = validator;
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

    protected override Task<Result<Guid>> HandleAuthorizedCoreAsync(
        CreateMachineCommand command,
        SecurityContext securityContext,
        CancellationToken cancellationToken)
    {
        var machineName = MachineName.Create(command.Name);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var machine = Machine.Create(machineName, changeContext);

        return Task.FromResult(Result<Guid>.Success(machine.Id));
    }
}