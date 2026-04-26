using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M200_Business.M204_Inventory.Stock;

namespace SWFC.Application.M200_Business.M204_Inventory.Stock;

public sealed class CreateStockMovementValidator : ICommandValidator<CreateStockMovementCommand>
{
    public Task<ValidationResult> ValidateAsync(
        CreateStockMovementCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        ValidateRequiredFields(command, errors);
        ValidateMovementType(command, errors);
        ValidateTransfer(command, errors);
        ValidateTraceability(command, errors);

        return Task.FromResult(
            errors.Count == 0
                ? ValidationResult.Success()
                : ValidationResult.Failure(errors.ToArray()));
    }

    private static void ValidateRequiredFields(
        CreateStockMovementCommand command,
        List<ValidationError> errors)
    {
        if (command.InventoryItemId == Guid.Empty)
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Inventory item is required."));
        }

        if (command.LocationId == Guid.Empty)
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Location is required."));
        }

        if (!string.IsNullOrWhiteSpace(command.Bin) && command.Bin.Trim().Length > 100)
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Bin must not exceed 100 characters."));
        }
    }

    private static void ValidateMovementType(
        CreateStockMovementCommand command,
        List<ValidationError> errors)
    {
        if (!Enum.IsDefined(command.MovementType))
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Movement type is invalid."));
        }

        if (command.QuantityDelta == 0)
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Quantity delta must not be zero."));
        }

        if (command.MovementType == StockMovementType.GoodsReceipt && command.QuantityDelta <= 0)
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "GoodsReceipt requires a positive quantity delta."));
        }

        if ((command.MovementType == StockMovementType.GoodsIssue
                || command.MovementType == StockMovementType.Consumption)
            && command.QuantityDelta >= 0)
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, $"{command.MovementType} requires a negative quantity delta."));
        }
    }

    private static void ValidateTransfer(
        CreateStockMovementCommand command,
        List<ValidationError> errors)
    {
        if (command.MovementType == StockMovementType.Transfer && command.QuantityDelta >= 0)
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Transfer requires a negative source quantity delta."));
        }

        if (command.MovementType == StockMovementType.Transfer
            && (!command.TransferLocationId.HasValue || command.TransferLocationId.Value == Guid.Empty))
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Transfer target location is required."));
        }

        if (command.MovementType == StockMovementType.Transfer
            && command.TransferLocationId == command.LocationId
            && string.Equals(NormalizeBin(command.TransferBin), NormalizeBin(command.Bin), StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Transfer target must differ from source stock."));
        }

        if (!string.IsNullOrWhiteSpace(command.TransferBin) && command.TransferBin.Trim().Length > 100)
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Transfer bin must not exceed 100 characters."));
        }
    }

    private static void ValidateTraceability(
        CreateStockMovementCommand command,
        List<ValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Reason is required."));
        }

        if (!string.IsNullOrWhiteSpace(command.TargetReference) && command.TargetReference.Trim().Length > 200)
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Target reference must not exceed 200 characters."));
        }

        if (command.TargetType is null && !string.IsNullOrWhiteSpace(command.TargetReference))
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Target reference requires target type."));
        }

        if (command.TargetType is not null && string.IsNullOrWhiteSpace(command.TargetReference))
        {
            errors.Add(new ValidationError(ValidationErrorCodes.Invalid, "Target type requires target reference."));
        }
    }

    private static string? NormalizeBin(string? bin)
    {
        return string.IsNullOrWhiteSpace(bin) ? null : bin.Trim();
    }
}

public sealed class CreateStockMovementPolicy : IAuthorizationPolicy<CreateStockMovementCommand>
{
    public AuthorizationRequirement GetRequirement(CreateStockMovementCommand request)
    {
        return new AuthorizationRequirement(requiredPermissions: new[] { "stockmovement.create" });
    }
}
