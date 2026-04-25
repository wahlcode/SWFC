using SWFC.Domain.M200_Business.M204_Inventory.Errors;
using System.Text.Json;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;

namespace SWFC.Application.M200_Business.M204_Inventory.Items;

public sealed record DeactivateInventoryItemCommand(
    Guid Id,
    string Reason);

public sealed class DeactivateInventoryItemValidator : ICommandValidator<DeactivateInventoryItemCommand>
{
    public Task<ValidationResult> ValidateAsync(
        DeactivateInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ValidationErrorCodes.Invalid, "Inventory item id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(InventoryErrorCodes.ReasonRequired, "Reason is required.");
        }

        return Task.FromResult(result);
    }
}

public sealed class DeactivateInventoryItemPolicy : IAuthorizationPolicy<DeactivateInventoryItemCommand>
{
    public AuthorizationRequirement GetRequirement(DeactivateInventoryItemCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "inventoryitem.deactivate" });
    }
}

public sealed class DeactivateInventoryItemHandler : IUseCaseHandler<DeactivateInventoryItemCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IInventoryItemWriteRepository _inventoryItemWriteRepository;
    private readonly IAuditService _auditService;

    public DeactivateInventoryItemHandler(
        ICurrentUserService currentUserService,
        IInventoryItemWriteRepository inventoryItemWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _inventoryItemWriteRepository = inventoryItemWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        DeactivateInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var inventoryItem = await _inventoryItemWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (inventoryItem is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"InventoryItem '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            inventoryItem.Id,
            Name = inventoryItem.Name.Value,
            inventoryItem.IsActive,
            inventoryItem.AuditInfo.CreatedAtUtc,
            inventoryItem.AuditInfo.CreatedBy,
            inventoryItem.AuditInfo.LastModifiedAtUtc,
            inventoryItem.AuditInfo.LastModifiedBy,
            command.Reason
        });

        _inventoryItemWriteRepository.Deactivate(inventoryItem, changeContext);

        var newValues = JsonSerializer.Serialize(new
        {
            inventoryItem.Id,
            IsActive = false,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "DeactivateInventoryItem",
            entity: "InventoryItem",
            entityId: inventoryItem.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

        await _inventoryItemWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}


