using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Web.Pages.M100_System.M106_Theme.Services;
using SWFC.Web.Pages.M300_Presentation.M307_User_Personalization.Models;

namespace SWFC.Web.Pages.M300_Presentation.M307_User_Personalization.Services;

public sealed class UserUiPreferenceService
{
    private const string CookieName = ".SWFC.M307.UserUiPreferences";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserService _currentUserService;
    private readonly ThemeCatalogService _themeCatalogService;
    private readonly IDataProtector _protector;

    public UserUiPreferenceService(
        IHttpContextAccessor httpContextAccessor,
        ICurrentUserService currentUserService,
        ThemeCatalogService themeCatalogService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentUserService = currentUserService;
        _themeCatalogService = themeCatalogService;
        _protector = dataProtectionProvider.CreateProtector("SWFC.M307.UserUiPreferences.v1");
    }

    public async Task<UserUiPreferences> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var securityContext = await _currentUserService.GetSecurityContextAsync(cancellationToken);
        var httpContext = _httpContextAccessor.HttpContext;

        return Read(securityContext, httpContext);
    }

    public UserUiPreferences Read(SecurityContext securityContext, HttpContext? httpContext)
    {
        if (httpContext is null ||
            !httpContext.Request.Cookies.TryGetValue(CookieName, out var protectedValue) ||
            string.IsNullOrWhiteSpace(protectedValue))
        {
            return UserUiPreferences.Default;
        }

        try
        {
            var json = _protector.Unprotect(protectedValue);
            var envelope = JsonSerializer.Deserialize<UserUiPreferenceEnvelope>(json);

            if (envelope is null ||
                !string.Equals(envelope.UserId, securityContext.UserId, StringComparison.OrdinalIgnoreCase))
            {
                return UserUiPreferences.Default;
            }

            return UserUiPreferences.Normalize(envelope.Preferences, _themeCatalogService);
        }
        catch
        {
            return UserUiPreferences.Default;
        }
    }

    public void Write(
        HttpContext httpContext,
        SecurityContext securityContext,
        UserUiPreferences preferences)
    {
        var normalized = UserUiPreferences.Normalize(preferences, _themeCatalogService);
        var envelope = new UserUiPreferenceEnvelope(securityContext.UserId, normalized);
        var json = JsonSerializer.Serialize(envelope);
        var protectedValue = _protector.Protect(json);

        httpContext.Response.Cookies.Append(
            CookieName,
            protectedValue,
            new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = ShouldIssueSecureCookie(httpContext.Request),
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });
    }

    private static bool ShouldIssueSecureCookie(HttpRequest request)
    {
        if (request.IsHttps)
        {
            return true;
        }

        var host = request.Host.Host;

        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !IPAddress.TryParse(host, out var address) || !IPAddress.IsLoopback(address);
    }
}
