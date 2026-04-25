using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M700_Support.M706_SLA_Service_Levels;

namespace SWFC.Application.M700_Support.M706_SLA_Service_Levels;

public sealed record CreateServiceLevelCommand(
    string Priority,
    TimeSpan ResponseTime,
    TimeSpan ProcessingTime,
    bool UseForSupport,
    bool UseForIncidentManagement,
    string Reason);

public sealed class CreateServiceLevelValidator : ICommandValidator<CreateServiceLevelCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateServiceLevelCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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

public sealed class CreateServiceLevelPolicy : IAuthorizationPolicy<CreateServiceLevelCommand>
{
    public AuthorizationRequirement GetRequirement(CreateServiceLevelCommand request) =>
        new(requiredPermissions: new[] { "support.write" });
}

public sealed class CreateServiceLevelHandler : IUseCaseHandler<CreateServiceLevelCommand, Guid>
{
    private readonly IServiceLevelWriteRepository _serviceLevelWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public CreateServiceLevelHandler(
        IServiceLevelWriteRepository serviceLevelWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _serviceLevelWriteRepository = serviceLevelWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateServiceLevelCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var serviceLevel = ServiceLevel.Create(
            command.Priority,
            command.ResponseTime,
            command.ProcessingTime,
            command.UseForSupport,
            command.UseForIncidentManagement,
            changeContext);

        await _serviceLevelWriteRepository.AddAsync(serviceLevel, cancellationToken);
        await _serviceLevelWriteRepository.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "CreateServiceLevel",
                Module: "M706",
                ObjectType: "ServiceLevel",
                ObjectId: serviceLevel.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
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

        return Result<Guid>.Success(serviceLevel.Id);
    }
}
