using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using SWFC.Application;
using SWFC.Infrastructure.DependencyInjection;
using SWFC.Infrastructure.M800_Security.Auth;
using SWFC.Infrastructure.M800_Security.Auth.Configuration;
using SWFC.Infrastructure.Services.System;
using SwfcAuthenticationOptions = SWFC.Infrastructure.M800_Security.Auth.Configuration.AuthenticationOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();

var authenticationOptions = builder.Configuration
    .GetSection(SwfcAuthenticationOptions.SectionName)
    .Get<SwfcAuthenticationOptions>() ?? new SwfcAuthenticationOptions();

if (string.Equals(authenticationOptions.Mode, AuthenticationModes.Local, StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = authenticationOptions.Local.CookieName;
            options.LoginPath = "/auth/login";
            options.AccessDeniedPath = "/auth/access-denied";
            options.LogoutPath = "/auth/logout";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(authenticationOptions.Local.SessionTimeoutMinutes);
            options.SlidingExpiration = true;
        });
}
else if (string.Equals(authenticationOptions.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate();
}
else
{
    throw new InvalidOperationException(
        $"Unsupported authentication mode '{authenticationOptions.Mode}'.");
}

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddScoped(_ =>
{
    var navigationManager = _.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IM102DataInitializer>();
    await initializer.InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<global::SWFC.Web.App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/auth/login");
})
.RequireAuthorization()
.DisableAntiforgery();

app.Run();