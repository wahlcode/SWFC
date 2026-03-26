namespace SWFC.Application.Common.Validation;

public sealed class ValidationResult
{
    private readonly List<ValidationError> _errors = new();

    public IReadOnlyCollection<ValidationError> Errors => _errors;
    public bool IsValid => _errors.Count == 0;

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(params ValidationError[] errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var result = new ValidationResult();
        result._errors.AddRange(errors.Where(x => x is not null));
        return result;
    }

    public void Add(string code, string message)
    {
        _errors.Add(new ValidationError(code, message));
    }
}
