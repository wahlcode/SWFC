using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M703_SupportCases;

namespace SWFC.Application.M700_Support.M703_SupportCases;

public sealed record UpdateSupportCaseCommand(
    Guid Id,
    string UserRequest,
    string ProblemDescription,
    string Reason);

public sealed class UpdateSupportCaseValidator : ICommandValidator<UpdateSupportCaseCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateSupportCaseCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Support case id is required.");
        }

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

public sealed class UpdateSupportCasePolicy : IAuthorizationPolicy<UpdateSupportCaseCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateSupportCaseCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class UpdateSupportCaseHandler : IUseCaseHandler<UpdateSupportCaseCommand, bool>
{
    private readonly ISupportCaseWriteRepository _supportCaseWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdateSupportCaseHandler(
        ISupportCaseWriteRepository supportCaseWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _supportCaseWriteRepository = supportCaseWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateSupportCaseCommand command,
        CancellationToken cancellationToken = default)
    {
        var supportCase = await _supportCaseWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (supportCase is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"SupportCase '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        var oldValues = JsonSerializer.Serialize(new
        {
            supportCase.Id,
            supportCase.UserRequest,
            supportCase.ProblemDescription,
            supportCase.TriggeredBugId,
            supportCase.TriggeredIncidentId
        });

        supportCase.UpdateDetails(
            command.UserRequest,
            command.ProblemDescription,
            changeContext);

        await _supportCaseWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "UpdateSupportCase",
                Module: "M703",
                ObjectType: "SupportCase",
                ObjectId: supportCase.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                OldValues: oldValues,
                NewValues: JsonSerializer.Serialize(new
                {
                    supportCase.Id,
                    supportCase.UserRequest,
                    supportCase.ProblemDescription,
                    supportCase.TriggeredBugId,
                    supportCase.TriggeredIncidentId
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<bool>.Success(true);
    }
}
