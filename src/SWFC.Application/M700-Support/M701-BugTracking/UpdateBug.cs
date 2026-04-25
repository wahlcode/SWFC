using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M701_BugTracking;

namespace SWFC.Application.M700_Support.M701_BugTracking;

public sealed record UpdateBugCommand(
    Guid Id,
    string Description,
    string Reproducibility,
    BugStatus Status,
    string Reason);

public sealed class UpdateBugValidator : ICommandValidator<UpdateBugCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateBugCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Bug id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            result.Add("Description", "Description is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reproducibility))
        {
            result.Add("Reproducibility", "Reproducibility is required.");
        }

        if (!Enum.IsDefined(command.Status))
        {
            result.Add("Status", "Status is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateBugPolicy : IAuthorizationPolicy<UpdateBugCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateBugCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class UpdateBugHandler : IUseCaseHandler<UpdateBugCommand, bool>
{
    private readonly IBugWriteRepository _bugWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdateBugHandler(
        IBugWriteRepository bugWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _bugWriteRepository = bugWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateBugCommand command,
        CancellationToken cancellationToken = default)
    {
        var bug = await _bugWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (bug is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Bug '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        var oldValues = JsonSerializer.Serialize(new
        {
            bug.Id,
            bug.Description,
            bug.Reproducibility,
            Status = bug.Status.ToString()
        });

        bug.UpdateDetails(
            command.Description,
            command.Reproducibility,
            command.Status,
            changeContext);

        await _bugWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "UpdateBug",
                Module: "M701",
                ObjectType: "Bug",
                ObjectId: bug.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                OldValues: oldValues,
                NewValues: JsonSerializer.Serialize(new
                {
                    bug.Id,
                    bug.Description,
                    bug.Reproducibility,
                    Status = bug.Status.ToString()
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<bool>.Success(true);
    }
}
