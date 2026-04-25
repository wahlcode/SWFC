using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;

namespace SWFC.Application.M200_Business.M204_Inventory.Locations;

public sealed record UpdateLocationCommand(
    Guid Id,
    string Name,
    string Code,
    Guid? ParentLocationId,
    string Reason);

public sealed class UpdateLocationValidator : ICommandValidator<UpdateLocationCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        if (command.Id == Guid.Empty)
        {
            errors.Add(new ValidationError(
                "Location.Id.Required",
                "Id is required."));
        }

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

public sealed class UpdateLocationPolicy : IAuthorizationPolicy<UpdateLocationCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateLocationCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "location.update" });
    }
}

public sealed class UpdateLocationHandler : IUseCaseHandler<UpdateLocationCommand, bool>
{
    private readonly ILocationWriteRepository _locationWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public UpdateLocationHandler(
        ILocationWriteRepository locationWriteRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _locationWriteRepository = locationWriteRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        var location = await _locationWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (location is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"Location '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            location.Id,
            Name = location.Name.Value,
            Code = location.Code.Value,
            location.ParentLocationId,
            location.AuditInfo.CreatedAtUtc,
            location.AuditInfo.CreatedBy,
            location.AuditInfo.LastModifiedAtUtc,
            location.AuditInfo.LastModifiedBy
        });

        location.Update(
            LocationName.Create(command.Name),
            LocationCode.Create(command.Code),
            command.ParentLocationId,
            changeContext);

        var newValues = JsonSerializer.Serialize(new
        {
            location.Id,
            Name = location.Name.Value,
            Code = location.Code.Value,
            location.ParentLocationId,
            location.AuditInfo.CreatedAtUtc,
            location.AuditInfo.CreatedBy,
            location.AuditInfo.LastModifiedAtUtc,
            location.AuditInfo.LastModifiedBy,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "UpdateLocation",
            entity: "Location",
            entityId: location.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _locationWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
