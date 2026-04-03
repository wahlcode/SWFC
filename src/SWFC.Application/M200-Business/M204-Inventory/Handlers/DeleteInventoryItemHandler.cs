using System.Text.Json;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class DeleteInventoryItemHandler : IUseCaseHandler<DeleteInventoryItemCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IInventoryItemWriteRepository _inventoryItemWriteRepository;
    private readonly IAuditService _auditService;

    public DeleteInventoryItemHandler(
        ICurrentUserService currentUserService,
        IInventoryItemWriteRepository inventoryItemWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _inventoryItemWriteRepository = inventoryItemWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(
        DeleteInventoryItemCommand command,
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
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            inventoryItem.Id,
            Name = inventoryItem.Name.Value,
            inventoryItem.AuditInfo.CreatedAtUtc,
            inventoryItem.AuditInfo.CreatedBy,
            inventoryItem.AuditInfo.LastModifiedAtUtc,
            inventoryItem.AuditInfo.LastModifiedBy,
            command.Reason
        });

        _inventoryItemWriteRepository.Remove(inventoryItem);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "DeleteInventoryItem",
            entity: "InventoryItem",
            entityId: inventoryItem.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: oldValues,
            newValues: null,
            cancellationToken: cancellationToken);

        await _inventoryItemWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}