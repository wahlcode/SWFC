namespace SWFC.Domain.M100_System.M101_Foundation.Exceptions;

public sealed class ConflictException : SwfcException
{
    public ConflictException(string message) : base(message)
    {
    }
}

