namespace SWFC.Domain.Common.Exceptions;

public sealed class SecurityException : SwfcException
{
    public SecurityException(string message) : base(message)
    {
    }
}