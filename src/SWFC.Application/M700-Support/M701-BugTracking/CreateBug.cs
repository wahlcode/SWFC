using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M701_BugTracking;

namespace SWFC.Application.M700_Support.M701_BugTracking;

public sealed record CreateBugCommand(
    string Description,
    string Reproducibility,
    BugStatus Status,
    string Reason);

public sealed class CreateBugValidator : ICommandValidator<CreateBugCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateBugCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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

public sealed class CreateBugPolicy : IAuthorizationPolicy<CreateBugCommand>
{
    public AuthorizationRequirement GetRequirement(CreateBugCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class CreateBugHandler : IUseCaseHandler<CreateBugCommand, Guid>
{
    private readonly IBugWriteRepository _bugWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public CreateBugHandler(
        IBugWriteRepository bugWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _bugWriteRepository = bugWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateBugCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var bug = Bug.Create(
            command.Description,
            command.Reproducibility,
            command.Status,
            changeContext);

        await _bugWriteRepository.AddAsync(bug, cancellationToken);
        await _bugWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "CreateBug",
                Module: "M701",
                ObjectType: "Bug",
                ObjectId: bug.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    bug.Id,
                    bug.Description,
                    bug.Reproducibility,
                    Status = bug.Status.ToString()
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<Guid>.Success(bug.Id);
    }
}
