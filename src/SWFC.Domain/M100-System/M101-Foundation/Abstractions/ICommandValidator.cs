using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Domain.M100_System.M101_Foundation.Abstractions;

public interface ICommandValidator<in TCommand>
{
    Task<ValidationResult> ValidateAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}

