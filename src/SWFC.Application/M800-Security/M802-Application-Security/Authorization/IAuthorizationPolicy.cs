namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;

public interface IAuthorizationPolicy<in TRequest>
{
    AuthorizationRequirement GetRequirement(TRequest request);
}

