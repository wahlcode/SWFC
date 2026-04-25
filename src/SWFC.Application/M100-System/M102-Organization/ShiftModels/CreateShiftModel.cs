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

public sealed record CreateShiftModelCommand(
    string Name,
    string Code,
    string? Description,
    string Reason);

public sealed class CreateShiftModelValidator : ICommandValidator<CreateShiftModelCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateShiftModelCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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

public sealed class CreateShiftModelPolicy : IAuthorizationPolicy<CreateShiftModelCommand>
{
    public AuthorizationRequirement GetRequirement(CreateShiftModelCommand request) =>
        new(requiredPermissions: new[] { "organization.write" });
}

public sealed class CreateShiftModelHandler : IUseCaseHandler<CreateShiftModelCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IShiftModelWriteRepository _shiftModelWriteRepository;
    private readonly IAuditService _auditService;

    public CreateShiftModelHandler(
        ICurrentUserService currentUserService,
        IShiftModelWriteRepository shiftModelWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _shiftModelWriteRepository = shiftModelWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateShiftModelCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingShiftModel = await _shiftModelWriteRepository.GetByCodeAsync(
            command.Code.Trim(),
            cancellationToken);

        if (existingShiftModel is not null)
        {
            return Result<Guid>.Failure(
                new Error(
                    "m102.shift_model.code.exists",
                    "A shift model with the same code already exists.",
                    ErrorCategory.Conflict));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var shiftModel = ShiftModel.Create(
            ShiftModelName.Create(command.Name),
            ShiftModelCode.Create(command.Code),
            command.Description,
            changeContext);

        await _shiftModelWriteRepository.AddAsync(shiftModel, cancellationToken);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.DisplayName,
            action: "CreateShiftModel",
            entity: "ShiftModel",
            entityId: shiftModel.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                shiftModel.Id,
                Name = shiftModel.Name.Value,
                Code = shiftModel.Code.Value,
                shiftModel.Description,
                shiftModel.IsActive,
                command.Reason
            }),
            cancellationToken: cancellationToken);

        await _shiftModelWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(shiftModel.Id);
    }
}
