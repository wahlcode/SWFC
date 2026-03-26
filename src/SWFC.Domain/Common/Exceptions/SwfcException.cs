namespace SWFC.Domain.Common.Exceptions;

public abstract class SwfcException : Exception
{
    protected SwfcException(string message) : base(message)
    {
    }

    protected SwfcException(string message, Exception innerException) : base(message, innerException)
    {
    }
}