using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;

namespace SWFC.Application.M200_Business.M204_Inventory.Locations;

public sealed record CreateLocationCommand(
    string Name,
    string Code,
    Guid? ParentLocationId,
    string Reason);

public sealed class CreateLocationValidator : ICommandValidator<CreateLocationCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            errors.Add(new ValidationError(
                "Location.Name.Required",
                "Name is required."));
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            errors.Add(new ValidationError(
                "Location.Code.Required",
                "Code is required."));
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            errors.Add(new ValidationError(
                "Location.Reason.Required",
                "Reason is required."));
        }

        return Task.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors.ToArray()));
    }
}

public sealed class CreateLocationPolicy : IAuthorizationPolicy<CreateLocationCommand>
{
    public AuthorizationRequirement GetRequirement(CreateLocationCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "location.create" });
    }
}

public sealed class CreateLocationHandler : IUseCaseHandler<CreateLocationCommand, Guid>
{
    private readonly ILocationWriteRepository _repo;
    private readonly ICurrentUserService _user;
    private readonly IAuditService _auditService;

    public CreateLocationHandler(
        ILocationWriteRepository repo,
        ICurrentUserService user,
        IAuditService auditService)
    {
        _repo = repo;
        _user = user;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateLocationCommand command,
        CancellationToken ct = default)
    {
        var securityContext = await _user.GetSecurityContextAsync(ct);
        var ctx = ChangeContext.Create(securityContext.UserId, command.Reason);

        var location = Location.Create(
            LocationName.Create(command.Name),
            LocationCode.Create(command.Code),
            command.ParentLocationId,
            ctx);

        await _repo.AddAsync(location, ct);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "CreateLocation",
            entity: "Location",
            entityId: location.Id.ToString(),
            timestampUtc: ctx.ChangedAtUtc,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                location.Id,
                Name = location.Name.Value,
                Code = location.Code.Value,
                location.ParentLocationId,
                command.Reason
            }),
            cancellationToken: ct);

        await _repo.SaveChangesAsync(ct);

        return Result<Guid>.Success(location.Id);
    }
}
