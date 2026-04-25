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
using SWFC.Domain.M200_Business.M204_Inventory.Stock;
using SWFC.Domain.M200_Business.M204_Inventory.Reservations;

namespace SWFC.Application.M200_Business.M204_Inventory.Items;

public sealed record CreateInventoryItemCommand(
    string ArticleNumber,
    string Name,
    string Description,
    string Unit,
    string? Barcode,
    string? Manufacturer,
    string? ManufacturerPartNumber,
    bool IsActive,
    string Reason);

public sealed class CreateInventoryItemValidator : ICommandValidator<CreateInventoryItemCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateInventoryItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

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

        return Task.FromResult(result);
    }
}

public sealed class CreateInventoryItemPolicy : IAuthorizationPolicy<CreateInventoryItemCommand>
{
    public AuthorizationRequirement GetRequirement(CreateInventoryItemCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "inventoryitem.create" });
    }
}

public sealed class CreateInventoryItemHandler : IUseCaseHandler<CreateInventoryItemCommand, Guid>
{
    private readonly IInventoryItemWriteRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public CreateInventoryItemHandler(
        IInventoryItemWriteRepository repository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<Result<Guid>> HandleAsync(
        CreateInventoryItemCommand command,
        CancellationToken ct = default)
    {
        var user = await _currentUserService.GetSecurityContextAsync(ct);
        var ctx = ChangeContext.Create(user.UserId, command.Reason);

        var item = InventoryItem.Create(
            InventoryItemArticleNumber.Create(command.ArticleNumber),
            InventoryItemName.Create(command.Name),
            InventoryItemDescription.Create(command.Description),
            InventoryItemUnit.Create(command.Unit),
            InventoryItemBarcode.CreateOptional(command.Barcode),
            InventoryItemManufacturer.CreateOptional(command.Manufacturer),
            InventoryItemManufacturerPartNumber.CreateOptional(command.ManufacturerPartNumber), ctx);

        await _repository.AddAsync(item, ct);

        await _auditService.WriteAsync(
            userId: user.UserId,
            username: user.Username,
            action: "CreateInventoryItem",
            entity: "InventoryItem",
            entityId: item.Id.ToString(),
            timestampUtc: ctx.ChangedAtUtc,
            oldValues: null,
            newValues: JsonSerializer.Serialize(new
            {
                item.Id,
                ArticleNumber = item.ArticleNumber.Value,
                Name = item.Name.Value,
                Description = item.Description.Value,
                Unit = item.Unit.Value,
                Barcode = item.Barcode?.Value,
                Manufacturer = item.Manufacturer?.Value,
                ManufacturerPartNumber = item.ManufacturerPartNumber?.Value,
                item.IsActive,
                command.Reason
            }),
            cancellationToken: ct);

        await _repository.SaveChangesAsync(ct);

        return Result<Guid>.Success(item.Id);
    }
}



