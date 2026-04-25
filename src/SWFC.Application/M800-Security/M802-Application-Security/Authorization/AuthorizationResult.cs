using SWFC.Domain.M800_Security.M801_Security_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Errors;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

public sealed class AuthorizationResult
{
    private AuthorizationResult(bool isAuthorized, Error error)
    {
        IsAuthorized = isAuthorized;
        Error = error;
    }

    public bool IsAuthorized { get; }
    public Error Error { get; }

    public static AuthorizationResult Authorized() =>
        new(true, Error.None);

    public static AuthorizationResult Forbidden(string message) =>
        new(
            false,
            new Error(
                GeneralErrorCodes.Unauthorized,
                message,
                ErrorCategory.Security));
}


