using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M705_Knowledge_Base;

namespace SWFC.Application.M700_Support.M705_Knowledge_Base;

public sealed record CreateKnowledgeEntryCommand(
    KnowledgeEntryType Type,
    string Content,
    string Reason);

public sealed class CreateKnowledgeEntryValidator : ICommandValidator<CreateKnowledgeEntryCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateKnowledgeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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

public sealed class CreateKnowledgeEntryPolicy : IAuthorizationPolicy<CreateKnowledgeEntryCommand>
{
    public AuthorizationRequirement GetRequirement(CreateKnowledgeEntryCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class CreateKnowledgeEntryHandler : IUseCaseHandler<CreateKnowledgeEntryCommand, Guid>
{
    private readonly IKnowledgeEntryWriteRepository _knowledgeEntryWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public CreateKnowledgeEntryHandler(
        IKnowledgeEntryWriteRepository knowledgeEntryWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _knowledgeEntryWriteRepository = knowledgeEntryWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateKnowledgeEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var knowledgeEntry = KnowledgeEntry.Create(
            command.Type,
            command.Content,
            changeContext);

        await _knowledgeEntryWriteRepository.AddAsync(knowledgeEntry, cancellationToken);
        await _knowledgeEntryWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "CreateKnowledgeEntry",
                Module: "M705",
                ObjectType: "KnowledgeEntry",
                ObjectId: knowledgeEntry.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    knowledgeEntry.Id,
                    Type = knowledgeEntry.Type.ToString(),
                    knowledgeEntry.Content
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<Guid>.Success(knowledgeEntry.Id);
    }
}
