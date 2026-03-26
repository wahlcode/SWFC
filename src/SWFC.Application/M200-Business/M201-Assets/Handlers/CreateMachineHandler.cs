using SWFC.Application.Common.Validation;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M201_Assets.Entities;
using SWFC.Domain.M200_Business.M201_Assets.ValueObjects;

namespace SWFC.Application.M200_Business.M201_Assets.Handlers;

public sealed class CreateMachineHandler
{
    private readonly ICommandValidator<CreateMachineCommand> _validator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthorizationPolicy<CreateMachineCommand> _authorizationPolicy;

    public CreateMachineHandler(
        ICommandValidator<CreateMachineCommand> validator,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IAuthorizationPolicy<CreateMachineCommand> authorizationPolicy)
    {
        _validator = validator;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _authorizationPolicy = authorizationPolicy;
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
                    code: "GEN_VALIDATION_FAILED",
                    message: message,
                    category: ErrorCategory.Validation));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var requirement = _authorizationPolicy.GetRequirement(command);
        var authorization = await _authorizationService.AuthorizeAsync(
            securityContext,
            requirement,
            cancellationToken);

        if (!authorization.IsAuthorized)
        {
            return Result<Guid>.Failure(authorization.Error);
        }

        var machineName = MachineName.Create(command.Name);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var machine = Machine.Create(machineName, changeContext);

        return Result<Guid>.Success(machine.Id);
    }
}