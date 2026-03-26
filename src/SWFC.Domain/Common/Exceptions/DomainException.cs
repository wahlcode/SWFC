namespace SWFC.Domain.Common.Exceptions;

public sealed class DomainException : SwfcException
{
    public DomainException(string message) : base(message)
    {
    }
}