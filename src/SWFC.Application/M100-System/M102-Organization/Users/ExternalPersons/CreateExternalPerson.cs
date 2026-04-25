using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.Users.ExternalPersons;

namespace SWFC.Application.M100_System.M102_Organization.Users.ExternalPersons;

public sealed record CreateExternalPersonCommand(
    string DisplayName,
    string CompanyName,
    string? Email,
    string? Phone,
    string? Function,
    Guid? OrganizationUnitId,
    string Reason);

public sealed class CreateExternalPersonValidator : ICommandValidator<CreateExternalPersonCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateExternalPersonCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (string.IsNullOrWhiteSpace(command.DisplayName))
            result.Add("DisplayName", "Display name is required.");

        if (string.IsNullOrWhiteSpace(command.CompanyName))
            result.Add("CompanyName", "Company name is required.");

        if (string.IsNullOrWhiteSpace(command.Reason))
            result.Add("Reason", "Reason is required.");

        if (command.OrganizationUnitId.HasValue && command.OrganizationUnitId.Value == Guid.Empty)
            result.Add("OrganizationUnitId", "Organization unit id is invalid.");

        return Task.FromResult(result);
    }
}

public sealed class CreateExternalPersonPolicy : IAuthorizationPolicy<CreateExternalPersonCommand>
{
    public AuthorizationRequirement GetRequirement(CreateExternalPersonCommand request) =>
        new(requiredPermissions: new[] { "organization.users.write" });
}

public sealed class CreateExternalPersonHandler : IUseCaseHandler<CreateExternalPersonCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IExternalPersonWriteRepository _externalPersonWriteRepository;
    private readonly IAuditService _auditService;

    public CreateExternalPersonHandler(
        ICurrentUserService currentUserService,
        IExternalPersonWriteRepository externalPersonWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _externalPersonWriteRepository = externalPersonWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateExternalPersonCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var externalPerson = ExternalPerson.Create(
            command.DisplayName,
            command.CompanyName,
            command.Email,
            command.Phone,
            command.Function,
            command.OrganizationUnitId,
            changeContext);

        await _externalPersonWriteRepository.AddAsync(externalPerson, cancellationToken);

        await _auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: securityContext.UserId,
                ActorDisplayName: securityContext.DisplayName,
                Action: "ExternalPersonChanged",
                Module: "M102",
                ObjectType: "ExternalPerson",
                ObjectId: externalPerson.Id.ToString(),
                TimestampUtc: changeContext.ChangedAtUtc,
                NewValues: JsonSerializer.Serialize(new
                {
                    ChangeType = "Created",
                    externalPerson.Id,
                    externalPerson.DisplayName,
                    externalPerson.CompanyName,
                    externalPerson.Email,
                    externalPerson.Phone,
                    externalPerson.Function,
                    externalPerson.OrganizationUnitId,
                    externalPerson.IsActive
                }),
                Reason: command.Reason),
            cancellationToken);

        await _externalPersonWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(externalPerson.Id);
    }
}