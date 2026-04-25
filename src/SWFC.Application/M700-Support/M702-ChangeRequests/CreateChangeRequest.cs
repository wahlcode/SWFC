using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M702_ChangeRequests;

namespace SWFC.Application.M700_Support.M702_ChangeRequests;

public sealed record CreateChangeRequestCommand(
    ChangeRequestType Type,
    string Description,
    string? RequirementReference,
    string? RoadmapReference,
    string Reason);

public sealed class CreateChangeRequestValidator : ICommandValidator<CreateChangeRequestCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateChangeRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (!Enum.IsDefined(command.Type))
        {
            result.Add("Type", "Type is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            result.Add("Description", "Description is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class CreateChangeRequestPolicy : IAuthorizationPolicy<CreateChangeRequestCommand>
{
    public AuthorizationRequirement GetRequirement(CreateChangeRequestCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class CreateChangeRequestHandler : IUseCaseHandler<CreateChangeRequestCommand, Guid>
{
    private readonly IChangeRequestWriteRepository _changeRequestWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public CreateChangeRequestHandler(
        IChangeRequestWriteRepository changeRequestWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _changeRequestWriteRepository = changeRequestWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateChangeRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var changeRequest = ChangeRequest.Create(
            command.Type,
            command.Description,
            command.RequirementReference,
            command.RoadmapReference,
            changeContext);

        await _changeRequestWriteRepository.AddAsync(changeRequest, cancellationToken);
        await _changeRequestWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "CreateChangeRequest",
                Module: "M702",
                ObjectType: "ChangeRequest",
                ObjectId: changeRequest.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    changeRequest.Id,
                    Type = changeRequest.Type.ToString(),
                    changeRequest.Description,
                    changeRequest.RequirementReference,
                    changeRequest.RoadmapReference
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<Guid>.Success(changeRequest.Id);
    }
}
