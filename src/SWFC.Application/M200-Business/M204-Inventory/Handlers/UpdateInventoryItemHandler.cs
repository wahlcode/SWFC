using System.Text.Json;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.ValueObjects;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class UpdateInventoryItemHandler : IUseCaseHandler<UpdateInventoryItemCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IInventoryItemWriteRepository _inventoryItemWriteRepository;
    private readonly IAuditService _auditService;

    public UpdateInventoryItemHandler(
        ICurrentUserService currentUserService,
        IInventoryItemWriteRepository inventoryItemWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _inventoryItemWriteRepository = inventoryItemWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        UpdateInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var inventoryItem = await _inventoryItemWriteRepository.GetByIdAsync(command.Id, cancellationToken);

        if (inventoryItem is null)
        {
            return Result<bool>.Failure(new Error(
                ErrorCodes.General.NotFound,
                $"InventoryItem '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var oldValues = JsonSerializer.Serialize(new
        {
            inventoryItem.Id,
            Name = inventoryItem.Name.Value,
            inventoryItem.AuditInfo.CreatedAtUtc,
            inventoryItem.AuditInfo.CreatedBy,
            inventoryItem.AuditInfo.LastModifiedAtUtc,
            inventoryItem.AuditInfo.LastModifiedBy
        });

        var inventoryItemName = InventoryItemName.Create(command.Name);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        inventoryItem.Rename(inventoryItemName, changeContext);

        var newValues = JsonSerializer.Serialize(new
        {
            inventoryItem.Id,
            Name = inventoryItem.Name.Value,
            inventoryItem.AuditInfo.CreatedAtUtc,
            inventoryItem.AuditInfo.CreatedBy,
            inventoryItem.AuditInfo.LastModifiedAtUtc,
            inventoryItem.AuditInfo.LastModifiedBy,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "UpdateInventoryItem",
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