using SWFC.Domain.Common.Results;

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

    public async Task<Result<TResult>> HandleAuthorizedAsync(
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var requirement = _authorizationPolicy.GetRequirement(request);
        var authorizationResult = await _authorizationService.AuthorizeAsync(
            securityContext,
            requirement,
            cancellationToken);

        if (!authorizationResult.IsAuthorized)
        {
            return Result<TResult>.Failure(authorizationResult.Error);
        }

        return await HandleCoreAsync(request, securityContext, cancellationToken);
    }

    protected abstract Task<Result<TResult>> HandleCoreAsync(
        TRequest request,
        SecurityContext securityContext,
        CancellationToken cancellationToken);
}