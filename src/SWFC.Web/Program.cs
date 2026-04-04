using Microsoft.AspNetCore.Authentication.Negotiate;
using SWFC.Application;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Infrastructure.DependencyInjection;
using SWFC.Infrastructure.M800_Security.Auth;
using SWFC.Infrastructure.M800_Security.Auth.Configuration;
using SWFC.Infrastructure.M800_Security.Auth.Providers.Sso;
using SWFC.Infrastructure.Services.System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var authenticationOptions = builder.Configuration
    .GetSection(AuthenticationOptions.SectionName)
    .Get<AuthenticationOptions>() ?? new AuthenticationOptions();

if (string.Equals(authenticationOptions.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate();

    builder.Services.AddAuthorization();
    builder.Services.AddScoped<ICurrentUserService, SsoCurrentUserService>();
}

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

if (string.Equals(authenticationOptions.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapRazorComponents<global::SWFC.Web.App>()
    .AddInteractiveServerRenderMode();

app.Run();