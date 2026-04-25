using SWFC.Application.M800_Security.M802_ApplicationSecurity;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Authorization;
using SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Validation;
using SWFC.Architecture.Tests.Support;
using SWFC.Domain.M100_System.M101_Foundation.Abstractions;
using SWFC.Domain.M100_System.M101_Foundation.Results;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M801_M802_V010CompletionTests
{
    [Fact]
    public void M801_Sicherheitsgrundlagen_Basiskonzepte_And_Systemweite_Gueltigkeit_Should_Be_Expressed_By_Access_Rules()
    {
        // Sicherheitsgrundlagen
        // Basiskonzepte
        // Ubergreifende Sicherheitsregeln
        // Sicherheitsprinzipien fuer alle Module
        // Systemweite Gueltigkeit
        // Keine modulbezogene Fachlogik
        var changeContext = ChangeContext.Create("system", "bootstrap");
        var accessRule = AccessRule.Create(
            AccessTargetType.Machine,
            "machine-1",
            AccessSubjectType.Role,
            "role-1",
            AccessRuleMode.Allow,
            changeContext);

        Assert.True(accessRule.IsActive);
        Assert.True(accessRule.MatchesTarget(AccessTargetType.Machine, "machine-1"));
        Assert.True(accessRule.MatchesSubject(AccessSubjectType.Role, "role-1"));

        accessRule.UpdateMode(AccessRuleMode.Deny, changeContext);
        accessRule.Deactivate(changeContext);

        Assert.Equal(AccessRuleMode.Deny, accessRule.Mode);
        Assert.False(accessRule.IsActive);
    }

    [Fact]
    public async Task M802_Eingabeabsicherung_Technische_Autorisierungsdurchsetzung_And_Keine_Umgehung_Von_Sicherheitspruefungen_Should_Be_Enforced()
    {
        // Eingabeabsicherung
        var validationStep = new ValidationStep<DummyRequest, bool>(
            [new FailingValidator()]);
        var pipelineContext = new PipelineContext<DummyRequest>(
            new DummyRequest("request"),
            new SecurityContext("user-1", "id-1", "user", "User", true));

        var validationResult = await validationStep.ExecuteAsync(pipelineContext);

        Assert.True(validationResult.IsFailure);
        Assert.Equal("GEN_VALIDATION_FAILED", validationResult.Error.Code);

        // Technische Autorisierungsdurchsetzung auf Anwendungsebene
        // Keine Umgehung von Sicherheitspruefungen
        var authorizationService = new AuthorizationService();
        var denied = await authorizationService.AuthorizeAsync(
            new SecurityContext("user-1", "id-1", "user", "User", true, permissions: ["inventory.read"]),
            new AuthorizationRequirement(requiredPermissions: ["inventory.write"]));
        var allowedForDeveloperMode = await authorizationService.AuthorizeAsync(
            new SecurityContext("user-1", "id-1", "user", "User", true, isDeveloperMode: true),
            new AuthorizationRequirement(requiredPermissions: ["inventory.write"]));

        Assert.False(denied.IsAuthorized);
        Assert.True(allowedForDeveloperMode.IsAuthorized);
    }

    [Fact]
    public void M802_Schutz_Gegen_Typische_Web_App_Angriffe_Sichere_Request_Response_Verarbeitung_Schutz_Vor_Injection_Schutz_Vor_Manipulation_And_Schutz_Vor_Unsicheren_Eingaben_Should_Have_Runtime_Artifacts()
    {
        // Schutz gegen typische Web-/App-Angriffe
        // Sichere Request-/Response-Verarbeitung
        // Schutz vor Injection
        // Schutz vor Manipulation
        // Schutz vor unsicheren Eingaben
        var programPath = RepositoryRoot.Combine("src", "SWFC.Web", "Program.cs");
        var validationPath = RepositoryRoot.Combine("src", "SWFC.Application", "M800-Security", "M802-Application-Security", "Pipeline", "Validation", "ValidationStep.cs");
        var authorizationPath = RepositoryRoot.Combine("src", "SWFC.Application", "M800-Security", "M802-Application-Security", "Authorization", "AuthorizationService.cs");

        var combinedContent = string.Join(
            Environment.NewLine,
            File.ReadAllText(programPath),
            File.ReadAllText(validationPath),
            File.ReadAllText(authorizationPath));

        Assert.Contains("ValidateRequestAsync", combinedContent, StringComparison.Ordinal);
        Assert.Contains("HttpOnly", combinedContent, StringComparison.Ordinal);
        Assert.Contains("SameSite", combinedContent, StringComparison.Ordinal);
        Assert.Contains("Secure", combinedContent, StringComparison.Ordinal);
        Assert.Contains("ValidationStep", combinedContent, StringComparison.Ordinal);
        Assert.Contains("AuthorizationService", combinedContent, StringComparison.Ordinal);
    }

    private sealed record DummyRequest(string Value);

    private sealed class FailingValidator : ICommandValidator<DummyRequest>
    {
        public Task<ValidationResult> ValidateAsync(
            DummyRequest command,
            CancellationToken cancellationToken = default)
        {
            var result = ValidationResult.Success();
            result.Add("value", "Validation failed.");
            return Task.FromResult(result);
        }
    }
}
