using SWFC.Domain.M200_Business.M204_Inventory.Errors;
using System.Text.Json;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M204_Inventory.Items;
using SWFC.Domain.M200_Business.M204_Inventory.Locations;

namespace SWFC.Application.M200_Business.M204_Inventory.Items;

public sealed record UpdateInventoryItemCommand(
    Guid Id,
    string ArticleNumber,
    string Name,
    string Description,
    string Unit,
    string? Barcode,
    string? Manufacturer,
    string? ManufacturerPartNumber,
    bool IsActive,
    string Reason,
    decimal StandardUnitPrice = 0,
    string Currency = "EUR");

public sealed class UpdateInventoryItemValidator : ICommandValidator<UpdateInventoryItemCommand>
{
    public Task<ValidationResult> ValidateAsync(
        UpdateInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ValidationErrorCodes.Invalid, "Inventory item id is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            result.Add(ValidationErrorCodes.Invalid, "Inventory item name is required.");
        }
        else if (command.Name.Trim().Length > 100)
        {
            result.Add(ValidationErrorCodes.Invalid, "Inventory item name must not exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            result.Add(InventoryErrorCodes.ReasonRequired, "Reason is required.");
        }

        if (command.StandardUnitPrice < 0)
        {
            result.Add(ValidationErrorCodes.Invalid, "Standard unit price must not be negative.");
        }

        return Task.FromResult(result);
    }
}

public sealed class UpdateInventoryItemPolicy : IAuthorizationPolicy<UpdateInventoryItemCommand>
{
    public AuthorizationRequirement GetRequirement(UpdateInventoryItemCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "inventoryitem.update" });
    }
}

public sealed class UpdateInventoryItemHandler : IUseCaseHandler<UpdateInventoryItemCommand, bool>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IInventoryItemWriteRepository _repo;
    private readonly IAuditService _auditService;

    public UpdateInventoryItemHandler(
        IInventoryItemWriteRepository repo,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _repo = repo;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<bool>> HandleAsync(UpdateInventoryItemCommand command, CancellationToken ct = default)
    {
        var item = await _repo.GetByIdAsync(command.Id, ct);

        if (item is null)
        {
            return Result<bool>.Failure(new Error(
                GeneralErrorCodes.NotFound,
                $"InventoryItem '{command.Id}' was not found.",
                ErrorCategory.NotFound));
        }

        var user = await _currentUserService.GetSecurityContextAsync(ct);
        var ctx = ChangeContext.Create(user.UserId, command.Reason);

        var oldValues = JsonSerializer.Serialize(new
        {
            item.Id,
            ArticleNumber = item.ArticleNumber.Value,
            Name = item.Name.Value,
            Description = item.Description.Value,
            Unit = item.Unit.Value,
            Barcode = item.Barcode?.Value,
            Manufacturer = item.Manufacturer?.Value,
            ManufacturerPartNumber = item.ManufacturerPartNumber?.Value,
            StandardUnitPrice = item.StandardUnitPrice.Value,
            Currency = item.Currency.Value,
            item.IsActive,
            item.AuditInfo.CreatedAtUtc,
            item.AuditInfo.CreatedBy,
            item.AuditInfo.LastModifiedAtUtc,
            item.AuditInfo.LastModifiedBy
        });

        item.UpdateMasterData(
            InventoryItemArticleNumber.Create(command.ArticleNumber),
            InventoryItemName.Create(command.Name),
            InventoryItemDescription.Create(command.Description),
            InventoryItemUnit.Create(command.Unit),
            InventoryItemBarcode.CreateOptional(command.Barcode),
            InventoryItemManufacturer.CreateOptional(command.Manufacturer),
            InventoryItemManufacturerPartNumber.CreateOptional(command.ManufacturerPartNumber),
            InventoryItemStandardUnitPrice.Create(command.StandardUnitPrice),
            InventoryItemCurrency.Create(command.Currency),
            command.IsActive,
            ctx);

        var newValues = JsonSerializer.Serialize(new
        {
            item.Id,
            ArticleNumber = item.ArticleNumber.Value,
            Name = item.Name.Value,
            Description = item.Description.Value,
            Unit = item.Unit.Value,
            Barcode = item.Barcode?.Value,
            Manufacturer = item.Manufacturer?.Value,
            ManufacturerPartNumber = item.ManufacturerPartNumber?.Value,
            StandardUnitPrice = item.StandardUnitPrice.Value,
            Currency = item.Currency.Value,
            item.IsActive,
            item.AuditInfo.CreatedAtUtc,
            item.AuditInfo.CreatedBy,
            item.AuditInfo.LastModifiedAtUtc,
            item.AuditInfo.LastModifiedBy,
            command.Reason
        });

        await _auditService.WriteAsync(
            userId: user.UserId,
            username: user.Username,
            action: "UpdateInventoryItem",
            entity: "InventoryItem",
            entityId: item.Id.ToString(),
            timestampUtc: ctx.ChangedAtUtc,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: ct);

        await _repo.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}



