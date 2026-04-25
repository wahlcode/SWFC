using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

public abstract class AuthorizedHandler<TRequest, TResult>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthorizationPolicy<TRequest> _authorizationPolicy;

    protected AuthorizedHandler(
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IAuthorizationPolicy<TRequest> authorizationPolicy)
    {
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _authorizationPolicy = authorizationPolicy;
    }

    protected async Task<Result<TResult>> AuthorizeAndHandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var requirement = _authorizationPolicy.GetRequirement(request);
        var authorization = await _authorizationService.AuthorizeAsync(
            securityContext,
            requirement,
            cancellationToken);

        if (!authorization.IsAuthorized)
        {
            return Result<TResult>.Failure(authorization.Error);
        }

        return await HandleAuthorizedCoreAsync(request, securityContext, cancellationToken);
    }

    protected abstract Task<Result<TResult>> HandleAuthorizedCoreAsync(
        TRequest request,
        SecurityContext securityContext,
        CancellationToken cancellationToken);
}

