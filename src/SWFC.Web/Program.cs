using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SWFC.Application;
using SWFC.Application.M100_System.M103_Authentication;
using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M200_Business.M209_Projects;
using SWFC.Application.M200_Business.M210_Customers;
using SWFC.Application.M200_Business.M211_Analytics;
using SWFC.Application.M200_Business.M212_Production;
using SWFC.Application.M200_Business.M213_Workforce;
using SWFC.Infrastructure.DependencyInjection;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Configuration;
using SWFC.Infrastructure.M100_System.M103_Authentication;
using SWFC.Infrastructure.M100_System.M103_Authentication.Configuration;
using SWFC.Infrastructure.M100_System.M103_Authentication.Services;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment;
using SWFC.Web.Pages.M100_System.M106_Theme.Services;
using SWFC.Web.Pages.M300_Presentation.M306_Localization.Services;
using SWFC.Web.Pages.M300_Presentation.M307_User_Personalization.Models;
using SWFC.Web.Pages.M300_Presentation.M307_User_Personalization.Services;
using SWFC.Web.Pages.M400_Integration.M402_API;
using SWFC.Infrastructure.Services.Security;
using SWFC.Web.Components.Layout;
using SWFC.Web.Components.ModuleOverview;
using SWFC.Web.Components.Roadmap;
using SWFC.Web.Pages.M100_System.M104_Documents.Models;
using SWFC.Web.Pages.M100_System.M104_Documents.Services;
using SWFC.Web.Pages.M100_System.M105_Configuration.Services;
using SWFC.Web.Pages.M300_Presentation.M302_Reporting.Models;
using SWFC.Web.Pages.M300_Presentation.M302_Reporting.Services;
using SWFC.Web.Pages.M300_Presentation.M303_Notification.Models;
using SWFC.Web.Pages.M300_Presentation.M303_Notification.Services;
using SWFC.Web.Pages.M300_Presentation.M304_Search.Services;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using SwfcAuthenticationOptions = SWFC.Infrastructure.M100_System.M103_Authentication.Configuration.AuthenticationOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SidebarConfigService>();
builder.Services.AddScoped<SWFC.Web.Pages.M300_Presentation.M306_Localization.Services.LocalizationState>();
builder.Services.AddScoped<ModuleAuditService>();
builder.Services.AddScoped<ModuleStatusService>();
builder.Services.AddScoped<RoadmapService>();

builder.Services.AddScoped<LocalizationTextProvider>();
builder.Services.AddScoped<CultureFormattingService>();
builder.Services.AddScoped<LanguageCatalogService>();
builder.Services.AddScoped<LocalizationValidationMessageCatalog>();
builder.Services.AddScoped<NavigationModuleLabelService>();
builder.Services.AddScoped<ThemeCatalogService>();
builder.Services.AddScoped<UserUiPreferenceService>();
builder.Services.AddSingleton<DocumentWorkspaceService>();
builder.Services.AddSingleton<ConfigurationWorkspaceService>();
builder.Services.AddSingleton<ReportingWorkspaceService>();
builder.Services.AddSingleton<NotificationWorkspaceService>();
builder.Services.AddSingleton<SearchWorkspaceService>();
builder.Services.AddSingleton<ProjectWorkspaceService>();
builder.Services.AddSingleton<CustomerWorkspaceService>();
builder.Services.AddSingleton<AnalyticsWorkspaceService>();
builder.Services.AddSingleton<ProductionWorkspaceService>();
builder.Services.AddSingleton<WorkforceWorkspaceService>();

var authenticationOptions = builder.Configuration
    .GetSection(SwfcAuthenticationOptions.SectionName)
    .Get<SwfcAuthenticationOptions>() ?? new SwfcAuthenticationOptions();
var identityIntegrationOptions = builder.Configuration
    .GetSection(IdentityIntegrationOptions.SectionName)
    .Get<IdentityIntegrationOptions>() ?? new IdentityIntegrationOptions();
var oidcOptions = identityIntegrationOptions.Oidc ?? new OidcProviderOptions();


if (string.Equals(authenticationOptions.Mode, AuthenticationModes.Local, StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.Cookie.Name = authenticationOptions.Local.CookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.LoginPath = "/auth/login";
            options.AccessDeniedPath = "/auth/access-denied";
            options.LogoutPath = "/auth/logout";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(authenticationOptions.Local.SessionTimeoutMinutes);
            options.SlidingExpiration = true;
            options.Events = new CookieAuthenticationEvents
            {
                OnSigningIn = context =>
                {
                    context.CookieOptions.Secure = ShouldIssueSecureCookie(context.HttpContext.Request);
                    return Task.CompletedTask;
                }
            };
        });
}
else if (string.Equals(authenticationOptions.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase))
{
    if (!oidcOptions.IsConfigured())
    {
        throw new InvalidOperationException(
            $"Authentication mode '{AuthenticationModes.Sso}' requires a configured '{IdentityIntegrationOptions.SectionName}:Oidc' section.");
    }

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.Name = authenticationOptions.Local.CookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            options.LoginPath = "/auth/login";
            options.AccessDeniedPath = "/auth/access-denied";
            options.LogoutPath = "/auth/logout";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(authenticationOptions.Local.SessionTimeoutMinutes);
            options.SlidingExpiration = true;
            options.Events = new CookieAuthenticationEvents
            {
                OnSigningIn = context =>
                {
                    context.CookieOptions.Secure = ShouldIssueSecureCookie(context.HttpContext.Request);
                    return Task.CompletedTask;
                }
            };
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = oidcOptions.Authority;
            options.ClientId = oidcOptions.ClientId;
            options.CallbackPath = oidcOptions.CallbackPath;
            options.SignedOutCallbackPath = oidcOptions.SignedOutCallbackPath;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.UsePkce = true;
            options.SaveTokens = false;
            options.GetClaimsFromUserInfoEndpoint = false;
            options.MapInboundClaims = false;
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

            if (!string.IsNullOrWhiteSpace(oidcOptions.ClientSecret))
            {
                options.ClientSecret = oidcOptions.ClientSecret;
            }

            options.Scope.Clear();

            foreach (var scope in oidcOptions.Scopes
                         .Where(scope => !string.IsNullOrWhiteSpace(scope))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                options.Scope.Add(scope);
            }

            options.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = async context =>
                {
                    var flowService = context.HttpContext.RequestServices.GetRequiredService<OidcAuthenticationFlowService>();

                    try
                    {
                        context.Principal = await flowService.CreateInternalPrincipalAsync(
                            context.Principal ?? throw new InvalidOperationException("OIDC principal is missing."),
                            context.HttpContext.RequestAborted);
                    }
                    catch (Exception ex)
                    {
                        context.Fail(ex.Message);
                    }
                },
                OnRemoteFailure = async context =>
                {
                    var flowService = context.HttpContext.RequestServices.GetRequiredService<OidcAuthenticationFlowService>();

                    await flowService.WriteRemoteFailureAsync(
                        actorDisplayName: oidcOptions.DisplayName,
                        objectId: oidcOptions.DisplayName,
                        reason: TrimForAudit(context.Failure?.Message ?? "Single Sign-On callback failed."),
                        cancellationToken: context.HttpContext.RequestAborted);

                    var redirectUrl = BuildLoginUrl(
                        GetSafeLocalReturnUrl(context.Properties?.RedirectUri),
                        "Single Sign-On fehlgeschlagen.");

                    context.HandleResponse();
                    context.Response.Redirect(redirectUrl);
                },
                OnAuthenticationFailed = async context =>
                {
                    var flowService = context.HttpContext.RequestServices.GetRequiredService<OidcAuthenticationFlowService>();

                    await flowService.WriteTokenFailureAsync(
                        actorDisplayName: oidcOptions.DisplayName,
                        objectId: oidcOptions.DisplayName,
                        reason: TrimForAudit(context.Exception.Message),
                        cancellationToken: context.HttpContext.RequestAborted);

                    context.HandleResponse();
                    context.Response.Redirect(BuildLoginUrl("/", "Token ist ungueltig oder abgelaufen."));
                }
            };
        });
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

builder.Services.AddScoped(serviceProvider =>
{
    var navigationManager = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();

    return new HttpClient
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IM107SetupInitializer>();
    await initializer.InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<global::SWFC.Web.App>()
    .AddInteractiveServerRenderMode();

app.MapM402IntegrationApi();

app.MapGet("/auth/oidc/login", async (
    HttpContext httpContext,
    IAuditService auditService) =>
{
    if (!string.Equals(authenticationOptions.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase))
    {
        return Results.Redirect("/auth/login");
    }

    var returnUrl = GetSafeLocalReturnUrl(httpContext.Request.Query["returnUrl"]);

    await auditService.WriteAsync(
        new AuditWriteRequest(
            ActorUserId: "anonymous",
            ActorDisplayName: oidcOptions.DisplayName,
            Action: "SsoLoginStarted",
            Module: "M103",
            ObjectType: "Authentication",
            ObjectId: oidcOptions.DisplayName,
            TimestampUtc: DateTime.UtcNow,
            Reason: "OIDC login challenge started."),
        httpContext.RequestAborted);

    return Results.Challenge(
        new AuthenticationProperties
        {
            RedirectUri = returnUrl
        },
        [OpenIdConnectDefaults.AuthenticationScheme]);
})
.AllowAnonymous();

app.MapPost("/auth/login/submit", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    ClaimsPrincipalFactory claimsPrincipalFactory,
    IAuditService auditService,
    IUseCaseHandler<AuthenticateLocalUserCommand, AuthenticationResultDto> authenticateLocalUserHandler) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync();
    var username = form["Username"].ToString();
    var password = form["Password"].ToString();
    var returnUrl = GetSafeLocalReturnUrl(form["ReturnUrl"].ToString());
    var normalizedUsername = string.IsNullOrWhiteSpace(username)
        ? string.Empty
        : username.Trim();
    var rememberMe = form["RememberMe"].Any(value =>
        string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(value, "on", StringComparison.OrdinalIgnoreCase));
    var isBreakGlassLogin = IsBreakGlassLogin(authenticationOptions, normalizedUsername);

    if (!IsLocalLoginAllowed(authenticationOptions, normalizedUsername))
    {
        await auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: "anonymous",
                ActorDisplayName: string.IsNullOrWhiteSpace(normalizedUsername) ? "anonymous" : normalizedUsername,
                Action: "LoginFailed",
                Module: "M103",
                ObjectType: "Authentication",
                ObjectId: string.IsNullOrWhiteSpace(normalizedUsername) ? "anonymous" : normalizedUsername,
                TimestampUtc: DateTime.UtcNow,
                Reason: "Local login is not enabled in the current authentication mode."),
            httpContext.RequestAborted);

        return Results.LocalRedirect(
            BuildLoginUrl(
                returnUrl,
                "Lokale Anmeldung ist nicht freigegeben."));
    }

    var useCaseResult = await authenticateLocalUserHandler.HandleAsync(
        new AuthenticateLocalUserCommand(username, password));

    if (useCaseResult.IsFailure || useCaseResult.Value is null)
    {
        return Results.LocalRedirect(
            BuildLoginUrl(
                returnUrl,
                useCaseResult.Error?.Message ?? "Anmeldung fehlgeschlagen."));
    }

    var result = useCaseResult.Value;

    if (!result.Succeeded)
    {
        return Results.LocalRedirect(
            BuildLoginUrl(
                returnUrl,
                result.FailureReason ?? "Anmeldung fehlgeschlagen.",
                result.LockoutUntilUtc));
    }

    var principal = claimsPrincipalFactory.Create(
        result,
        CookieAuthenticationDefaults.AuthenticationScheme);

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            AllowRefresh = true,
            ExpiresUtc = rememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : null
        });

    await auditService.WriteAsync(
        new AuditWriteRequest(
            ActorUserId: result.UserId?.ToString() ?? "anonymous",
            ActorDisplayName: result.DisplayName,
            Action: "SessionCreated",
            Module: "M103",
            ObjectType: "AuthenticationSession",
            ObjectId: result.UserId?.ToString() ?? "anonymous",
            TimestampUtc: DateTime.UtcNow,
            TargetUserId: result.UserId?.ToString(),
            Reason: "Internal SWFC session created after local authentication."),
        httpContext.RequestAborted);

    if (isBreakGlassLogin)
    {
        await auditService.WriteAsync(
            new AuditWriteRequest(
                ActorUserId: result.UserId?.ToString() ?? "anonymous",
                ActorDisplayName: result.DisplayName,
                Action: "BreakGlassLoginUsed",
                Module: "M103",
                ObjectType: "Authentication",
                ObjectId: result.UserId?.ToString() ?? normalizedUsername,
                TimestampUtc: DateTime.UtcNow,
                TargetUserId: result.UserId?.ToString(),
                Reason: "Protected SuperAdmin used the local emergency access in SSO mode."),
            httpContext.RequestAborted);
    }

    return Results.LocalRedirect(returnUrl);
})
.AllowAnonymous();

app.MapPost("/auth/logout", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var timestampUtc = DateTime.UtcNow;
    var actorUserId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? "anonymous";
    var actorDisplayName = httpContext.User.FindFirst(SecurityClaimTypes.DisplayName)?.Value
        ?? httpContext.User.Identity?.Name
        ?? "anonymous";

    await auditService.WriteAsync(
        new AuditWriteRequest(
            ActorUserId: actorUserId,
            ActorDisplayName: actorDisplayName,
            Action: "Logout",
            Module: "M103",
            ObjectType: "AuthenticationSession",
            ObjectId: actorUserId,
            TimestampUtc: timestampUtc,
            TargetUserId: actorUserId,
            Reason: "User signed out."),
        default);

    await auditService.WriteAsync(
        new AuditWriteRequest(
            ActorUserId: actorUserId,
            ActorDisplayName: actorDisplayName,
            Action: "SessionEnded",
            Module: "M103",
            ObjectType: "AuthenticationSession",
            ObjectId: actorUserId,
            TimestampUtc: timestampUtc,
            TargetUserId: actorUserId,
            Reason: "Internal SWFC session ended."),
        default);

    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect(
        string.Equals(authenticationOptions.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase)
            ? oidcOptions.PostLogoutRedirectPath
            : "/auth/login");
})
.RequireAuthorization();

app.MapPost("/security/developer-mode/enable", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    ICurrentUserService currentUserService,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    if (!string.Equals(authenticationOptions.Mode, AuthenticationModes.Local, StringComparison.OrdinalIgnoreCase))
    {
        return Results.Redirect("/auth/access-denied");
    }

    var securityContext = await currentUserService.GetSecurityContextAsync();

    if (!securityContext.CanUseDeveloperMode)
    {
        return Results.Redirect("/auth/access-denied");
    }

    await UpdateDeveloperModeAsync(httpContext, enable: true);

    await auditService.WriteAsync(
        new AuditWriteRequest(
            ActorUserId: securityContext.UserId,
            ActorDisplayName: securityContext.DisplayName,
            Action: "DeveloperModeChanged",
            Module: "M806",
            ObjectType: "DeveloperMode",
            ObjectId: securityContext.UserId,
            TimestampUtc: DateTime.UtcNow,
            OldValues: JsonSerializer.Serialize(new { Enabled = false }),
            NewValues: JsonSerializer.Serialize(new { Enabled = true }),
            Reason: "Developer mode enabled by the protected SuperAdmin."),
        default);

    return Results.Redirect(GetReturnUrl(httpContext));
})
.RequireAuthorization();

app.MapPost("/security/developer-mode/disable", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    ICurrentUserService currentUserService,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    if (!string.Equals(authenticationOptions.Mode, AuthenticationModes.Local, StringComparison.OrdinalIgnoreCase))
    {
        return Results.Redirect("/auth/access-denied");
    }

    var securityContext = await currentUserService.GetSecurityContextAsync();

    if (!securityContext.CanUseDeveloperMode)
    {
        return Results.Redirect("/auth/access-denied");
    }

    await UpdateDeveloperModeAsync(httpContext, enable: false);

    await auditService.WriteAsync(
        new AuditWriteRequest(
            ActorUserId: securityContext.UserId,
            ActorDisplayName: securityContext.DisplayName,
            Action: "DeveloperModeChanged",
            Module: "M806",
            ObjectType: "DeveloperMode",
            ObjectId: securityContext.UserId,
            TimestampUtc: DateTime.UtcNow,
            OldValues: JsonSerializer.Serialize(new { Enabled = true }),
            NewValues: JsonSerializer.Serialize(new { Enabled = false }),
            Reason: "Developer mode disabled by the protected SuperAdmin."),
        default);

    return Results.Redirect(GetReturnUrl(httpContext));
})
.RequireAuthorization();

app.MapPost("/presentation/preferences/save", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    ICurrentUserService currentUserService,
    UserUiPreferenceService preferenceService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    var securityContext = await currentUserService.GetSecurityContextAsync(httpContext.RequestAborted);
    var preferences = new UserUiPreferences(
        ThemeName: form["ThemeName"].ToString(),
        StartPageRoute: form["StartPageRoute"].ToString(),
        DashboardWidgetKeys: form["DashboardWidgetKeys"]
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray(),
        NavigationDensity: form["NavigationDensity"].ToString(),
        UseReducedMotion: form["UseReducedMotion"].Any(value =>
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "on", StringComparison.OrdinalIgnoreCase)));

    preferenceService.Write(httpContext, securityContext, preferences);

    return Results.Redirect(
        GetSafeLocalReturnUrl(form["ReturnUrl"].ToString()));
})
.RequireAuthorization();

app.MapPost("/system/documents/register", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    DocumentWorkspaceService documents,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    var document = documents.RegisterDocument(new DocumentRegistrationRequest(
        form["Title"].ToString(),
        form["Category"].ToString(),
        form["OwnerModule"].ToString(),
        form["RetentionPolicy"].ToString()));

    documents.AddVersion(new DocumentVersionRequest(
        document.Id,
        form["VersionFileName"].ToString(),
        form["VersionContentType"].ToString(),
        ReadLongFormValue(form["VersionSizeBytes"].ToString()),
        form["VersionChecksum"].ToString(),
        form["Reason"].ToString()));

    await WriteAuditAsync(httpContext, auditService, "DocumentRegistered", "M104", "Document", document.Id.ToString(), "Document registered with initial version.");

    return Results.Redirect("/system/documents");
})
.RequireAuthorization();

app.MapPost("/system/documents/version", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    DocumentWorkspaceService documents,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    var document = documents.AddVersion(new DocumentVersionRequest(
        ReadGuidFormValue(form["DocumentId"].ToString()),
        form["FileName"].ToString(),
        form["ContentType"].ToString(),
        ReadLongFormValue(form["SizeBytes"].ToString()),
        form["Checksum"].ToString(),
        form["Reason"].ToString()));

    await WriteAuditAsync(httpContext, auditService, "DocumentVersionAdded", "M104", "Document", document.Id.ToString(), "Document version added.");

    return Results.Redirect("/system/documents");
})
.RequireAuthorization();

app.MapPost("/system/documents/link", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    DocumentWorkspaceService documents,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    var document = documents.LinkDocument(new DocumentLinkRequest(
        ReadGuidFormValue(form["DocumentId"].ToString()),
        form["TargetModule"].ToString(),
        form["TargetType"].ToString(),
        form["TargetId"].ToString(),
        form["Relationship"].ToString()));

    await WriteAuditAsync(httpContext, auditService, "DocumentLinked", "M104", "Document", document.Id.ToString(), "Document linked to module object.");

    return Results.Redirect("/system/documents");
})
.RequireAuthorization();

app.MapPost("/system/configuration/setting", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    ConfigurationWorkspaceService configuration,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    var setting = configuration.SetSetting(
        form["Key"].ToString(),
        form["Value"].ToString(),
        form["Reason"].ToString());

    await WriteAuditAsync(httpContext, auditService, "ConfigurationSettingChanged", "M105", "ConfigurationSetting", setting.Key, "Configuration setting changed.");

    return Results.Redirect("/system/configuration");
})
.RequireAuthorization();

app.MapPost("/system/configuration/module", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    ConfigurationWorkspaceService configuration,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    var activation = configuration.SetModuleActivation(
        form["ModuleCode"].ToString(),
        form["IsEnabled"].Any(value => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)),
        form["Reason"].ToString());

    await WriteAuditAsync(httpContext, auditService, "ModuleActivationChanged", "M105", "ModuleActivation", activation.ModuleCode, "Module activation changed.");

    return Results.Redirect("/system/configuration");
})
.RequireAuthorization();

app.MapPost("/presentation/reports/register", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    ReportingWorkspaceService reports,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    var report = reports.Register(new ReportDefinitionRequest(
        form["Name"].ToString(),
        form["SourceModule"].ToString(),
        form["Visualization"].ToString(),
        form["FilterDefinition"].ToString(),
        form["ExportFormats"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)));

    await WriteAuditAsync(httpContext, auditService, "ReportDefinitionRegistered", "M302", "ReportDefinition", report.Id.ToString(), "Report definition registered.");

    return Results.Redirect("/presentation/reports");
})
.RequireAuthorization();

app.MapPost("/presentation/notifications/publish", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    NotificationWorkspaceService notifications,
    IAuditService auditService) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    var notification = notifications.Publish(new NotificationRequest(
        form["Title"].ToString(),
        form["Message"].ToString(),
        form["Severity"].ToString(),
        form["Priority"].ToString(),
        form["TargetKind"].ToString(),
        form["TargetValue"].ToString(),
        form["Channel"].ToString(),
        form["RelatedModule"].ToString(),
        form["RelatedObjectId"].ToString()));

    await WriteAuditAsync(httpContext, auditService, "NotificationPublished", "M303", "Notification", notification.Id.ToString(), "Notification published.");

    return Results.Redirect("/presentation/notifications");
})
.RequireAuthorization();

app.MapPost("/presentation/notifications/mark-read", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    NotificationWorkspaceService notifications) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    notifications.MarkRead(ReadGuidFormValue(form["NotificationId"].ToString()));

    return Results.Redirect("/presentation/notifications");
})
.RequireAuthorization();

app.MapPost("/presentation/notifications/complete", async (
    HttpContext httpContext,
    IAntiforgery antiforgery,
    NotificationWorkspaceService notifications) =>
{
    await antiforgery.ValidateRequestAsync(httpContext);

    var form = await httpContext.Request.ReadFormAsync(httpContext.RequestAborted);
    notifications.Complete(ReadGuidFormValue(form["NotificationId"].ToString()));

    return Results.Redirect("/presentation/notifications");
})
.RequireAuthorization();

app.Run();

static async Task UpdateDeveloperModeAsync(HttpContext httpContext, bool enable)
{
    var authenticateResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
    {
        return;
    }

    var updatedClaims = authenticateResult.Principal.Claims
        .Where(x => !string.Equals(x.Type, SecurityClaimTypes.DeveloperMode, StringComparison.Ordinal))
        .ToList();

    if (enable)
    {
        updatedClaims.Add(new Claim(SecurityClaimTypes.DeveloperMode, "true"));
    }

    var identity = new ClaimsIdentity(updatedClaims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        authenticateResult.Properties);
}

static string GetReturnUrl(HttpContext httpContext)
{
    var referer = httpContext.Request.Headers.Referer.ToString();

    if (Uri.TryCreate(referer, UriKind.Absolute, out var uri))
    {
        var relativeUrl = uri.PathAndQuery + uri.Fragment;

        if (!string.IsNullOrWhiteSpace(relativeUrl))
        {
            return relativeUrl;
        }
    }

    return "/";

}

static string GetSafeLocalReturnUrl(string? returnUrl)
{
    if (!string.IsNullOrWhiteSpace(returnUrl) &&
        returnUrl.StartsWith("/", StringComparison.Ordinal) &&
        !returnUrl.StartsWith("//", StringComparison.Ordinal) &&
        !returnUrl.StartsWith("/\\", StringComparison.Ordinal))
    {
        return returnUrl;
    }

    return "/";
}

static string BuildLoginUrl(
    string returnUrl,
    string error,
    DateTimeOffset? lockoutUntilUtc = null)
{
    var queryParameters = new Dictionary<string, string?>();

    if (!string.IsNullOrWhiteSpace(error))
    {
        queryParameters["error"] = error;
    }

    if (!string.IsNullOrWhiteSpace(returnUrl) && !string.Equals(returnUrl, "/", StringComparison.Ordinal))
    {
        queryParameters["ReturnUrl"] = returnUrl;
    }

    if (lockoutUntilUtc.HasValue)
    {
        queryParameters["lockoutUntilUtc"] = lockoutUntilUtc.Value.UtcDateTime.ToString("O");
    }

    return queryParameters.Count == 0
        ? "/auth/login"
        : QueryHelpers.AddQueryString("/auth/login", queryParameters);
}

static bool ShouldIssueSecureCookie(HttpRequest request)
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

static bool IsLocalLoginAllowed(
    SwfcAuthenticationOptions authenticationOptions,
    string username)
{
    return string.Equals(authenticationOptions.Mode, AuthenticationModes.Local, StringComparison.OrdinalIgnoreCase) ||
           IsBreakGlassLogin(authenticationOptions, username);
}

static bool IsBreakGlassLogin(
    SwfcAuthenticationOptions authenticationOptions,
    string username)
{
    return IsBreakGlassLoginEnabled(authenticationOptions) &&
           !string.IsNullOrWhiteSpace(username) &&
           string.Equals(
               username.Trim(),
               authenticationOptions.InitialSuperAdmin.Username,
               StringComparison.OrdinalIgnoreCase);
}

static bool IsBreakGlassLoginEnabled(SwfcAuthenticationOptions authenticationOptions)
{
    return string.Equals(authenticationOptions.Mode, AuthenticationModes.Sso, StringComparison.OrdinalIgnoreCase) &&
           !string.IsNullOrWhiteSpace(authenticationOptions.InitialSuperAdmin.Password);
}

static string TrimForAudit(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return "Unspecified authentication failure.";
    }

    return value.Length <= 512
        ? value
        : value[..512];
}

static Guid ReadGuidFormValue(string value)
{
    if (!Guid.TryParse(value, out var parsed))
    {
        throw new InvalidOperationException("Invalid form identifier.");
    }

    return parsed;
}

static long ReadLongFormValue(string value)
{
    if (!long.TryParse(value, out var parsed) || parsed <= 0)
    {
        throw new InvalidOperationException("Invalid numeric form value.");
    }

    return parsed;
}

static Task WriteAuditAsync(
    HttpContext httpContext,
    IAuditService auditService,
    string action,
    string module,
    string objectType,
    string objectId,
    string reason)
{
    var actorUserId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? "anonymous";
    var actorDisplayName = httpContext.User.FindFirst(SecurityClaimTypes.DisplayName)?.Value
        ?? httpContext.User.Identity?.Name
        ?? "anonymous";

    return auditService.WriteAsync(
        new AuditWriteRequest(
            ActorUserId: actorUserId,
            ActorDisplayName: actorDisplayName,
            Action: action,
            Module: module,
            ObjectType: objectType,
            ObjectId: objectId,
            TimestampUtc: DateTime.UtcNow,
            TargetUserId: actorUserId,
            Reason: reason),
        httpContext.RequestAborted);
}
