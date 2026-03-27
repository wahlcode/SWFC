using SWFC.Application.Common.Validation;
using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Validation;

public sealed class ValidationStep<TRequest, TResponse>
    : IPipelineStep<TRequest, TResponse>
{
    private readonly IEnumerable<ICommandValidator<TRequest>> _validators;

    public ValidationStep(IEnumerable<ICommandValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<Result> ExecuteAsync(
        IPipelineContext<TRequest> context,
        CancellationToken cancellationToken = default)
    {
        foreach (var validator in _validators)
        {
            var validation = await validator.ValidateAsync(context.Request, cancellationToken);

            if (!validation.IsValid)
            {
                var message = string.Join("; ", validation.Errors.Select(x => x.Message));

                return Result.Failure(
                    new Error(
                        ErrorCodes.General.ValidationFailed,
                        message,
                        ErrorCategory.Validation));
            }
        }

        return Result.Success();
    }
}
