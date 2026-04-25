using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M705_Knowledge_Base;

namespace SWFC.Application.M700_Support.M705_Knowledge_Base;

public sealed record UpdateKnowledgeEntryCommand(
    Guid Id,
    KnowledgeEntryType Type,
    string Content,
    string Reason);

public sealed class UpdateKnowledgeEntryValidator : ICommandValidator<UpdateKnowledgeEntryCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateKnowledgeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Knowledge entry id is required.");
        }

        if (!Enum.IsDefined(command.Type))
        {
            result.Add("Type", "Type is invalid.");
        }

        if (string.IsNullOrWhiteSpace(command.Content))
        {
            result.Add("Content", "Content is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateKnowledgeEntryPolicy : IAuthorizationPolicy<UpdateKnowledgeEntryCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateKnowledgeEntryCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class UpdateKnowledgeEntryHandler : IUseCaseHandler<UpdateKnowledgeEntryCommand, bool>
{
    private readonly IKnowledgeEntryWriteRepository _knowledgeEntryWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdateKnowledgeEntryHandler(
        IKnowledgeEntryWriteRepository knowledgeEntryWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _knowledgeEntryWriteRepository = knowledgeEntryWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateKnowledgeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var knowledgeEntry = await _knowledgeEntryWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (knowledgeEntry is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"KnowledgeEntry '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        var oldValues = JsonSerializer.Serialize(new
        {
            knowledgeEntry.Id,
            Type = knowledgeEntry.Type.ToString(),
            knowledgeEntry.Content
        });

        knowledgeEntry.UpdateDetails(
            command.Type,
            command.Content,
            changeContext);

        await _knowledgeEntryWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "UpdateKnowledgeEntry",
                Module: "M705",
                ObjectType: "KnowledgeEntry",
                ObjectId: knowledgeEntry.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                OldValues: oldValues,
                NewValues: JsonSerializer.Serialize(new
                {
                    knowledgeEntry.Id,
                    Type = knowledgeEntry.Type.ToString(),
                    knowledgeEntry.Content
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<bool>.Success(true);
    }
}
