using SWFC.Domain.Common.Results;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Authorization;

public sealed class AuthorizationStep<TRequest, TResponse>
    : IPipelineStep<TRequest, TResponse>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthorizationPolicy<TRequest> _authorizationPolicy;

    public AuthorizationStep(
        IAuthorizationService authorizationService,
        IAuthorizationPolicy<TRequest> authorizationPolicy)
    {
        _authorizationService = authorizationService;
        _authorizationPolicy = authorizationPolicy;
    }

    public async Task<Result> ExecuteAsync(
        IPipelineContext<TRequest> context,
        CancellationToken cancellationToken = default)
    {
        var requirement = _authorizationPolicy.GetRequirement(context.Request);

        var authorization = await _authorizationService.AuthorizeAsync(
            context.SecurityContext,
            requirement,
            cancellationToken);

        if (!authorization.IsAuthorized)
        {
            return Result.Failure(authorization.Error);
        }

        return Result.Success();
    }
}
