namespace SWFC.Domain.M100_System.M101_Foundation.Exceptions;

public sealed class NotFoundException : SwfcException
{
    public NotFoundException(string message) : base(message)
    {
    }
}

