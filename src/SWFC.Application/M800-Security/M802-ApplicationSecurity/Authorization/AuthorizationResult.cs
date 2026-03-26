using SWFC.Domain.Common.Errors;
using SWFC.Domain.Common.Results;

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
                ErrorCodes.General.Unauthorized,
                message,
                ErrorCategory.Security));
}