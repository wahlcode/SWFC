using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M706_SLA_Service_Levels;

namespace SWFC.Application.M700_Support.M706_SLA_Service_Levels;

public sealed record UpdateServiceLevelCommand(
    Guid Id,
    string Priority,
    TimeSpan ResponseTime,
    TimeSpan ProcessingTime,
    bool UseForSupport,
    bool UseForIncidentManagement,
    string Reason);

public sealed class UpdateServiceLevelValidator : ICommandValidator<UpdateServiceLevelCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateServiceLevelCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Service level id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Priority))
        {
            result.Add("Priority", "Priority is required.");
        }

        if (command.ResponseTime <= TimeSpan.Zero)
        {
            result.Add("ResponseTime", "Response time must be greater than zero.");
        }

        if (command.ProcessingTime <= TimeSpan.Zero)
        {
            result.Add("ProcessingTime", "Processing time must be greater than zero.");
        }

        if (!command.UseForSupport && !command.UseForIncidentManagement)
        {
            result.Add("Usage", "At least one usage must be selected.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateServiceLevelPolicy : IAuthorizationPolicy<UpdateServiceLevelCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateServiceLevelCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class UpdateServiceLevelHandler : IUseCaseHandler<UpdateServiceLevelCommand, bool>
{
    private readonly IServiceLevelWriteRepository _serviceLevelWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdateServiceLevelHandler(
        IServiceLevelWriteRepository serviceLevelWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _serviceLevelWriteRepository = serviceLevelWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateServiceLevelCommand command,
        CancellationToken cancellationToken = default)
    {
        var serviceLevel = await _serviceLevelWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (serviceLevel is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"ServiceLevel '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);
        var oldValues = JsonSerializer.Serialize(new
        {
            serviceLevel.Id,
            serviceLevel.Priority,
            serviceLevel.ResponseTime,
            serviceLevel.ProcessingTime,
            serviceLevel.UseForSupport,
            serviceLevel.UseForIncidentManagement
        });

        serviceLevel.UpdateDetails(
            command.Priority,
            command.ResponseTime,
            command.ProcessingTime,
            command.UseForSupport,
            command.UseForIncidentManagement,
            changeContext);

        await _serviceLevelWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "UpdateServiceLevel",
                Module: "M706",
                ObjectType: "ServiceLevel",
                ObjectId: serviceLevel.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                OldValues: oldValues,
                NewValues: JsonSerializer.Serialize(new
                {
                    serviceLevel.Id,
                    serviceLevel.Priority,
                    serviceLevel.ResponseTime,
                    serviceLevel.ProcessingTime,
                    serviceLevel.UseForSupport,
                    serviceLevel.UseForIncidentManagement
                }),
                Reason: command.Reason),
            cancellationToken);

        return Result<bool>.Success(true);
    }
}
