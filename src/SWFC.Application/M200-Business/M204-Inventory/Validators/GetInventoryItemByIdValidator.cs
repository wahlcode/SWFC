using SWFC.Application.Common.Validation;
using SWFC.Application.M200_Business.M204_Inventory.Queries;
using SWFC.Domain.Common.Errors;

namespace SWFC.Application.M200_Business.M204_Inventory.Validators;

public sealed class GetInventoryItemByIdValidator : ICommandValidator<GetInventoryItemByIdQuery>
{
    public Task<ValidationResult> ValidateAsync(
        GetInventoryItemByIdQuery command,
        CancellationToken cancellationToken = default)
    {
        var result = ValidationResult.Success();

        if (command.Id == Guid.Empty)
        {
            result.Add(ErrorCodes.Validation.Invalid, "Inventory item id is required.");
        }

        return Task.FromResult(result);
    }
}