using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M100_System.M102_Organization.ShiftModels;

namespace SWFC.Application.M100_System.M102_Organization.ShiftModels;

public sealed record UpdateShiftModelCommand(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    string Reason);

public sealed class UpdateShiftModelValidator : ICommandValidator<UpdateShiftModelCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateShiftModelCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add("Id", "Shift model id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add("Name", "Shift model name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            result.Add("Code", "Shift model code is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add("Reason", "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateShiftModelPolicy : IAuthorizationPolicy<UpdateShiftModelCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateShiftModelCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class UpdateShiftModelHandler : IUseCaseHandler<UpdateShiftModelCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IShiftModelWriteRepository _shiftModelWriteRepository;
    private readonly IAuditService _auditService;

    public UpdateShiftModelHandler(
        ICurrentUserService currentUserService,
        IShiftModelWriteRepository shiftModelWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _shiftModelWriteRepository = shiftModelWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateShiftModelCommand command,
        CancellationToken cancellationToken = default)
    {
        var shiftModel = await _shiftModelWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (shiftModel is null)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.shift_model.not_found",
                    "Shift model was not found.",
                    ErrorCategory.NotFound));
        }

        var existingByCode = await _shiftModelWriteRepository.GetByCodeAsync(
            command.Code.Trim(),
            cancellationToken);

        if (existingByCode is not null && existingByCode.Id != command.Id)
        {
            return Result<bool>.Failure(
                new Error(
                    "m102.shift_model.code.exists",
                    "A shift model with the same code already exists.",
                    ErrorCategory.Conflict));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            shiftModel.Id,
            Name = shiftModel.Name.Value,
            Code = shiftModel.Code.Value,
            shiftModel.Description,
            shiftModel.IsActive
        });

        shiftModel.UpdateDetails(
            ShiftModelName.Create(command.Name),
            ShiftModelCode.Create(command.Code),
            command.Description,
            changeContext);

        if (command.IsActive)
        {
            shiftModel.Activate(changeContext);
        }
        else
        {
            shiftModel.Deactivate(changeContext);
        }

        var newValues = JsonSerializer.Serialize(new
        {
            shiftModel.Id,
            Name = shiftModel.Name.Value,
            Code = shiftModel.Code.Value,
            shiftModel.Description,
            shiftModel.IsActive,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.DisplayName,
            action: "UpdateShiftModel",
            entity: "ShiftModel",
            entityId: shiftModel.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _shiftModelWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
