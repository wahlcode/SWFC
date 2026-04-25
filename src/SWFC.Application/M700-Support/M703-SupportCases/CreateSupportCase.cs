using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M703_SupportCases;

namespace SWFC.Application.M700_Support.M703_SupportCases;

public sealed record CreateSupportCaseCommand(
    string UserRequest,
    string ProblemDescription,
    string Reason);

public sealed class CreateSupportCaseValidator : ICommandValidator<CreateSupportCaseCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateSupportCaseCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.UserRequest))
        {
            result.Add("UserRequest", "User request is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ProblemDescription))
        {
            result.Add("ProblemDescription", "Problem description is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class CreateSupportCasePolicy : IAuthorizationPolicy<CreateSupportCaseCommand>
{
    public AuthorizationRequirement GetRequirement(CreateSupportCaseCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class CreateSupportCaseHandler : IUseCaseHandler<CreateSupportCaseCommand, Guid>
{
    private readonly ISupportCaseWriteRepository _supportCaseWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public CreateSupportCaseHandler(
        ISupportCaseWriteRepository supportCaseWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _supportCaseWriteRepository = supportCaseWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateSupportCaseCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var supportCase = SupportCase.Create(
            command.UserRequest,
            command.ProblemDescription,
            changeContext);

        await _supportCaseWriteRepository.AddAsync(supportCase, cancellationToken);
        await _supportCaseWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "CreateSupportCase",
                Module: "M703",
                ObjectType: "SupportCase",
                ObjectId: supportCase.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    supportCase.Id,
                    supportCase.UserRequest,
                    supportCase.ProblemDescription
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<Guid>.Success(supportCase.Id);
    }
}
