namespace SWFC.Domain.Common.Exceptions;

public sealed class NotFoundException : SwfcException
{
    public NotFoundException(string message) : base(message)
    {
    }
}