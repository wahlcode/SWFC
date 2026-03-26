namespace SWFC.Domain.Common.Results;

public enum ErrorCategory
{
    Validation = 1,
    Domain = 2,
    Security = 3,
    Conflict = 4,
    NotFound = 5,
    Technical = 6
}