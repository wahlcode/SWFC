namespace SWFC.Domain.Common.Errors;

public static class ErrorCodes
{
    public static class General
    {
        public const string ValidationFailed = "GEN_VALIDATION_FAILED";
        public const string NotFound = "GEN_NOT_FOUND";
        public const string Conflict = "GEN_CONFLICT";
        public const string Unexpected = "GEN_UNEXPECTED";
    }

    public static class Validation
    {
        public const string Required = "VAL_REQUIRED";
        public const string Invalid = "VAL_INVALID";
        public const string TooLong = "VAL_TOO_LONG";
    }

    public static class Security
    {
        public const string ContextRequired = "SEC_CONTEXT_REQUIRED";
        public const string Unauthorized = "SEC_UNAUTHORIZED";
        public const string Forbidden = "SEC_FORBIDDEN";
    }
}
