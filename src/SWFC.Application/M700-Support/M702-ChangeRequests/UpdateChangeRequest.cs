using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M702_ChangeRequests;

namespace SWFC.Application.M700_Support.M702_ChangeRequests;

public sealed record UpdateChangeRequestCommand(
    Guid Id,
    ChangeRequestType Type,
    string Description,
    string? RequirementReference,
    string? RoadmapReference,
    string Reason);

public sealed class UpdateChangeRequestValidator : ICommandValidator<UpdateChangeRequestCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateChangeRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Change request id is required.");
        }

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

public sealed class UpdateChangeRequestPolicy : IAuthorizationPolicy<UpdateChangeRequestCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateChangeRequestCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class UpdateChangeRequestHandler : IUseCaseHandler<UpdateChangeRequestCommand, bool>
{
    private readonly IChangeRequestWriteRepository _changeRequestWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdateChangeRequestHandler(
        IChangeRequestWriteRepository changeRequestWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _changeRequestWriteRepository = changeRequestWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateChangeRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        var changeRequest = await _changeRequestWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (changeRequest is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"ChangeRequest '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        var oldValues = JsonSerializer.Serialize(new
        {
            changeRequest.Id,
            Type = changeRequest.Type.ToString(),
            changeRequest.Description,
            changeRequest.RequirementReference,
            changeRequest.RoadmapReference
        });

        changeRequest.UpdateDetails(
            command.Type,
            command.Description,
            command.RequirementReference,
            command.RoadmapReference,
            changeContext);

        await _changeRequestWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "UpdateChangeRequest",
                Module: "M702",
                ObjectType: "ChangeRequest",
                ObjectId: changeRequest.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                OldValues: oldValues,
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

        return Result<bool>.Success(true);
    }
}
