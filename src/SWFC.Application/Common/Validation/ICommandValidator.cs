using SWFC.Application.Common.Validation;

namespace SWFC.Application.Common.Validation;

public interface ICommandValidator<in TCommand>
{
    Task<ValidationResult> ValidateAsync(TCommand command, CancellationToken cancellationToken = default);
}