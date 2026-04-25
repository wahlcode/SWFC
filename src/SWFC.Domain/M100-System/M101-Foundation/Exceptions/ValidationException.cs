namespace SWFC.Domain.M100_System.M101_Foundation.Exceptions;

public sealed class ValidationException : SwfcException
{
    public IReadOnlyCollection<string> Errors { get; }

    public ValidationException(string message)
        : base(message)
    {
        Errors = Array.Empty<string>();
    }

    public ValidationException(string message, IReadOnlyCollection<string> errors)
        : base(message)
    {
        Errors = errors;
    }
}

