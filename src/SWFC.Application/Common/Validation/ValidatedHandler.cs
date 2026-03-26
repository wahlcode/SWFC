using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;

namespace SWFC.Application.Common.Validation;

public abstract class ValidatedHandler<TCommand, TResult>
{
    private readonly ICommandValidator<TCommand> _validator;

    protected ValidatedHandler(ICommandValidator<TCommand> validator)
    {
        _validator = validator;
    }

    public async Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);

        if (!validation.IsValid)
        {
            var message = string.Join("; ", validation.Errors.Select(x => x.Message));
            return Result<TResult>.Failure(
                new Error(
                    ErrorCodes.General.ValidationFailed,
                    message,
                    ErrorCategory.Validation));
        }

        return await HandleValidatedAsync(command, cancellationToken);
    }

    protected abstract Task<Result<TResult>> HandleValidatedAsync(TCommand command, CancellationToken cancellationToken);
}