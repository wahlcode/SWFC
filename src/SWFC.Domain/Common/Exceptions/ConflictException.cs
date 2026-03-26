namespace SWFC.Domain.Common.Exceptions;

public sealed class ConflictException : SwfcException
{
    public ConflictException(string message) : base(message)
    {
    }
}