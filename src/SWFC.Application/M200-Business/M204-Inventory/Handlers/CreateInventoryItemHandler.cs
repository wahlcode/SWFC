using System.Text.Json;
using SWFC.Application.M200_Business.M204_Inventory.Commands;
using SWFC.Application.M200_Business.M204_Inventory.Interfaces;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.Common.Results;
using SWFC.Domain.Common.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Entities;
using SWFC.Domain.M200_Business.M204_Inventory.ValueObjects;

namespace SWFC.Application.M200_Business.M204_Inventory.Handlers;

public sealed class CreateInventoryItemHandler : IUseCaseHandler<CreateInventoryItemCommand, Guid>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IInventoryItemWriteRepository _inventoryItemWriteRepository;
    private readonly IAuditService _auditService;

    public CreateInventoryItemHandler(
        ICurrentUserService currentUserService,
        IInventoryItemWriteRepository inventoryItemWriteRepository,
        IAuditService auditService)
    {
        _currentUserService = currentUserService;
        _inventoryItemWriteRepository = inventoryItemWriteRepository;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);

        var inventoryItemName = InventoryItemName.Create(command.Name);
        var changeContext = ChangeContext.Create(securityContext.UserId, command.Reason);

        var inventoryItem = InventoryItem.Create(inventoryItemName, changeContext);

        await _inventoryItemWriteRepository.AddAsync(inventoryItem, cancellationToken);

        await _auditService.WriteAsync(
            userId: securityContext.UserId,
            username: securityContext.Username,
            action: "CreateInventoryItem",
            entity: "InventoryItem",
            entityId: inventoryItem.Id.ToString(),
            timestampUtc: changeContext.ChangedAtUtc,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                inventoryItem.Id,
                Name = inventoryItem.Name.Value,
                command.Reason
            }),
            cancellationToken: cancellationToken);

        await _inventoryItemWriteRepository.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(inventoryItem.Id);
    }
}