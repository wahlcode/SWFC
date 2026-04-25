using System.Security.Claims;
using Microsoft.Extensions.Options;
using SWFC.Application.M800_Security.M805_AuditCompliance.Interfaces;
using SWFC.Application.M800_Security.M806_AccessControl.Shared;
using SWFC.Architecture.Tests.Support;
using SWFC.Infrastructure.M100_System.M103_Authentication.Configuration;
using SWFC.Infrastructure.M100_System.M103_Authentication.Services;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Configuration;
using SWFC.Infrastructure.M400_Integration.M406_IdentityIntegration.Services;
using SWFC.Infrastructure.Services.Security;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M103_V010CompletionTests
{
    [Fact]
    public void M103_Passwortregeln_Should_Hash_And_Verify_Passwords()
    {
        // Passwortregeln
        var hasher = new PasswordHasher();
        var hash = hasher.HashPassword("TopSecret123!");

        Assert.True(hasher.VerifyPassword("TopSecret123!", hash));
        Assert.False(hasher.VerifyPassword("WrongPassword", hash));
    }

    [Fact]
    public async Task M103_SSO_Current_User_Claims_Session_And_MFA_Vorbereitet_Should_Create_Internal_Principal_And_Audit()
    {
        // SSO
        // Current User
        // Claims
        // Session
        // MFA (vorbereitet)
        var userId = Guid.NewGuid();
        var projectionService = new StubProjectionService(
            new M102SecurityProjection(
                userId,
                "swfc.dev@example.com",
                "swfc.dev",
                "SWFC Developer",
                "de-DE",
                true,
                ["Operator"],
                ["organization.read"],
                ["M100"]));

        var authenticationOptions = Options.Create(
            new AuthenticationOptions
            {
                Mode = AuthenticationModes.Sso,
                InitialSuperAdmin = new InitialSuperAdminOptions
                {
                    Username = "swfc.dev"
                }
            });
        var setupOptions = Options.Create(
            new M107SetupOptions
            {
                SuperAdminRoleName = "SuperAdmin",
                SuperAdminIdentityKey = "swfc.dev@example.com"
            });
        var identityOptions = Options.Create(
            new IdentityIntegrationOptions
            {
                Oidc = new OidcProviderOptions()
            });
        var auditService = new RecordingAuditService();
        var resolver = new OidcExternalIdentityResolver(identityOptions);
        var securityContextResolver = new M102SecurityContextResolver(
            projectionService,
            authenticationOptions,
            setupOptions);
        var flowService = new OidcAuthenticationFlowService(
            resolver,
            securityContextResolver,
            new ClaimsPrincipalFactory(),
            auditService);

        var externalPrincipal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim("sub", "oidc-subject-1"),
                    new Claim("email", "swfc.dev@example.com"),
                    new Claim("name", "SWFC Developer"),
                    new Claim("amr", "pwd"),
                    new Claim("amr", "mfa"),
                    new Claim("acr", "loa-2")
                ],
                "oidc"));

        var internalPrincipal = await flowService.CreateInternalPrincipalAsync(externalPrincipal);

        Assert.Equal(userId.ToString(), internalPrincipal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("swfc.dev", internalPrincipal.Identity?.Name);
        Assert.Equal("swfc.dev@example.com", internalPrincipal.FindFirstValue(SecurityClaimTypes.IdentityKey));
        Assert.Contains(
            internalPrincipal.FindAll(SecurityClaimTypes.AuthenticationMethod).Select(x => x.Value),
            value => value == "pwd");
        Assert.Contains(
            internalPrincipal.FindAll(SecurityClaimTypes.AuthenticationMethod).Select(x => x.Value),
            value => value == "mfa");
        Assert.Equal("loa-2", internalPrincipal.FindFirstValue(SecurityClaimTypes.AuthenticationContext));
        Assert.Contains(auditService.Requests, request => request.Action == "LoginSuccess");
        Assert.Contains(auditService.Requests, request => request.Action == "SessionCreated");
    }

    [Fact]
    public void M103_Login_Auth_Provider_And_Break_Glass_Runtime_Artifacts_Should_Be_Present()
    {
        // Login
        // Auth Provider
        var programPath = RepositoryRoot.Combine("src", "SWFC.Web", "Program.cs");
        var loginPagePath = RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M100-System", "M103-Authentication", "Login.razor");
        var combinedContent = File.ReadAllText(programPath) + Environment.NewLine + File.ReadAllText(loginPagePath);

        Assert.Contains("/auth/login/submit", combinedContent, StringComparison.Ordinal);
        Assert.Contains("/auth/oidc/login", combinedContent, StringComparison.Ordinal);
        Assert.Contains("OpenIdConnectResponseType.Code", combinedContent, StringComparison.Ordinal);
        Assert.Contains("UsePkce = true", combinedContent, StringComparison.Ordinal);
        Assert.Contains("SaveTokens = false", combinedContent, StringComparison.Ordinal);
        Assert.Contains("MapInboundClaims = false", combinedContent, StringComparison.Ordinal);
        Assert.Contains("SsoLoginStarted", combinedContent, StringComparison.Ordinal);
        Assert.Contains("BreakGlassLoginUsed", combinedContent, StringComparison.Ordinal);
        Assert.Contains("Lokaler Notfallzugang", combinedContent, StringComparison.Ordinal);
        Assert.Contains("InitialSuperAdmin.Password", combinedContent, StringComparison.Ordinal);
    }

    private sealed class StubProjectionService : IM102SecurityProjectionService
    {
        private readonly M102SecurityProjection? _projection;

        public StubProjectionService(M102SecurityProjection? projection)
        {
            _projection = projection;
        }

        public Task<M102SecurityProjection?> GetByIdentityKeyAsync(
            string identityKey,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_projection);
        }
    }

    private sealed class RecordingAuditService : IAuditService
    {
        public List<AuditWriteRequest> Requests { get; } = new();

        public Task WriteAsync(
            string userId,
            string username,
            string action,
            string entity,
            string entityId,
            DateTime timestampUtc,
            string? oldValues = null,
            string? newValues = null,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(
                new AuditWriteRequest(
                    userId,
                    username,
                    action,
                    "M103",
                    entity,
                    entityId,
                    timestampUtc,
                    oldValues,
                    newValues));

            return Task.CompletedTask;
        }

        public Task WriteAsync(
            AuditWriteRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.CompletedTask;
        }
    }
}
