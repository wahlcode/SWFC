using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M803_Visibility;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M200_Business.M201_Assets.Machines;

public sealed record GetVisibleMachineByIdQuery(Guid Id);

public sealed class GetVisibleMachineByIdValidator : ICommandValidator<GetVisibleMachineByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetVisibleMachineByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Machine id is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class GetVisibleMachineByIdPolicy : IAuthorizationPolicy<GetVisibleMachineByIdQuery>
{
    public AuthorizationRequirement GetRequirement(GetVisibleMachineByIdQuery request)
        => new(requiredPermissions: new[] { "machine.read" });
}

public sealed class GetVisibleMachineByIdHandler : IUseCaseHandler<GetVisibleMachineByIdQuery, MachineDetailsDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IMachineReadRepository _machineReadRepository;
    private readonly IVisibilityResolver _visibilityResolver;

    public GetVisibleMachineByIdHandler(
        ICurrentUserService currentUserService,
        IMachineReadRepository machineReadRepository,
        IVisibilityResolver visibilityResolver)
    {
        _currentUserService = currentUserService;
        _machineReadRepository = machineReadRepository;
        _visibilityResolver = visibilityResolver;
    }

    public async Task<Result<MachineDetailsDto>> HandleAsync(
        GetVisibleMachineByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var visibilityContext = VisibilityContext.Create(
            securityContext.UserId,
            securityContext.Roles,
            securityContext.OrganizationUnitIds);

        var canView = await _visibilityResolver.CanViewMachine(
            visibilityContext,
            command.Id,
            cancellationToken);

        if (!canView)
        {
            return Result<MachineDetailsDto>.Failure(new Error(
                GeneralErrorCodes.Unauthorized,
                "Machine is not visible for the current user.",
                ErrorCategory.Security));
        }

        var machine = await _machineReadRepository.GetByIdAsync(command.Id, cancellationToken);

        if (machine is null)
        {
            return Result<MachineDetailsDto>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Machine '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        return Result<MachineDetailsDto>.Success(machine);
    }
}
