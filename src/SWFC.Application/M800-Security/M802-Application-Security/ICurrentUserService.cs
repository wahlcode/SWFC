namespace SWFC.Application.M800_Security.M802_ApplicationSecurity;

public interface ICurrentUserService
{
    Task<SecurityContext> GetSecurityContextAsync(CancellationToken cancellationToken = default);
}

