namespace SWFC.Domain.M100_System.M101_Foundation.Exceptions;

public abstract class SwfcException : Exception
{
    protected SwfcException(string message) : base(message)
    {
    }

    protected SwfcException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

