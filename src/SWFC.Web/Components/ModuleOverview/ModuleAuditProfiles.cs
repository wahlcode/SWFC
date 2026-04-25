namespace SWFC.Web.Components.ModuleOverview;

internal sealed record ModuleAuditProfile(
    IReadOnlyDictionary<string, ModuleAuditEvidenceProfile> WorkItems);

internal sealed record ModuleAuditEvidenceProfile(
    IReadOnlyList<string> CodeTerms,
    IReadOnlyList<string> DocumentationTerms,
    IReadOnlyList<string> TestTerms);

internal static class ModuleAuditProfiles
{
    private static readonly IReadOnlyDictionary<string, ModuleAuditProfile> Profiles =
        new Dictionary<string, ModuleAuditProfile>(StringComparer.OrdinalIgnoreCase)
        {
            ["M101"] = new ModuleAuditProfile(
                new Dictionary<string, ModuleAuditEvidenceProfile>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Result / Error Handling"] = new(
                        ["Result", "Result.Success", "Result.Failure", "ErrorCategory"],
                        ["Result / Error Handling"],
                        ["Result / Error Handling"]),
                    ["Guards"] = new(
                        ["Guard", "AgainstNullOrWhiteSpace", "AgainstMaxLength"],
                        ["Guards"],
                        ["Guards"]),
                    ["ValueObjects / Basis-Primitives"] = new(
                        ["SystemId", "UtcTimestamp", "MeasurementUnit"],
                        ["ValueObjects", "Basis-Primitives"],
                        ["ValueObjects / Basis-Primitives"]),
                    ["Globale ID-Grundlagen"] = new(
                        ["SystemId", "Guid.NewGuid", "Guid.TryParse"],
                        ["globale ID-Grundlagen"],
                        ["Globale ID-Grundlagen"]),
                    ["Zeit-/Timestamp-Basis (UTC)"] = new(
                        ["UtcTimestamp", "ToUniversalTime", "UtcDateTime"],
                        ["Zeit-/Timestamp-Basis", "UTC"],
                        ["Zeit-/Timestamp-Basis (UTC)"]),
                    ["Einheiten-Basis"] = new(
                        ["MeasurementUnit", "Value"],
                        ["Einheiten-Basis"],
                        ["Einheiten-Basis"]),
                    ["Audit-Basisfelder"] = new(
                        ["AuditInfo", "CreatedAtUtc", "LastModifiedAtUtc"],
                        ["Audit-Basisfelder"],
                        ["Audit-Basisfelder"])
                }),
            ["M103"] = new ModuleAuditProfile(
                new Dictionary<string, ModuleAuditEvidenceProfile>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Login"] = new(
                        ["AuthenticateLocalUser", "/auth/login/submit", "Login.razor"],
                        ["Login-Start", "Login"],
                        ["Login"]),
                    ["Auth Provider"] = new(
                        ["IdentityIntegrationOptions", "OidcProviderOptions", "Authority", "ClientId"],
                        ["Provider-Metadaten", "OIDC-Endpunkte"],
                        ["Auth Provider"]),
                    ["SSO"] = new(
                        ["OpenIdConnect", "/auth/oidc/login", "OidcAuthenticationFlowService"],
                        ["SSO-Login", "OIDC-basierte SSO-Anmeldung"],
                        ["SSO"]),
                    ["Session"] = new(
                        ["CookieAuthenticationDefaults.AuthenticationScheme", "SessionCreated", "SessionEnded"],
                        ["Session-Erzeugung", "Session-Verwaltung"],
                        ["Session"]),
                    ["Current User"] = new(
                        ["ICurrentUserService", "M102SecurityContextResolver", "CurrentUserService"],
                        ["Current User"],
                        ["Current User"]),
                    ["Claims"] = new(
                        ["ClaimsPrincipalFactory", "SecurityClaimTypes", "ClaimTypes.NameIdentifier"],
                        ["ClaimsPrincipal-Erzeugung", "Claims"],
                        ["Claims"]),
                    ["Passwortregeln"] = new(
                        ["PasswordHasher", "MaxFailedAttempts", "LockoutMinutes", "ChangeOwnPassword"],
                        ["Passwortregeln"],
                        ["Passwortregeln"]),
                    ["MFA (vorbereitet)"] = new(
                        ["AuthenticationMethod", "AuthenticationContext", "amr", "acr"],
                        ["MFA-Vorbereitung"],
                        ["MFA (vorbereitet)"])
                }),
            ["M107"] = new ModuleAuditProfile(
                new Dictionary<string, ModuleAuditEvidenceProfile>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Setup-State / Bootstrap-Zustaende"] = new(
                        ["SetupState", "IsConfigured", "SetupCompleted", "DatabaseInitialized", "SetupInProgress"],
                        ["Setup-State", "Bootstrap-Zustande", "Setup-/Bootstrap-Zustande"],
                        ["Setup-State / Bootstrap-Zustaende"]),
                    ["Erstinstallation / Bootstrap-Kontext"] = new(
                        ["InitializeAsync", "startup bootstrap", "Erstinitialisierung"],
                        ["Erstinstallation", "Bootstrap"],
                        ["Erstinstallation / Bootstrap-Kontext"]),
                    ["DB-Verbindungspruefung"] = new(
                        ["CanConnectAsync", "DatabaseChecked"],
                        ["DB-Verbindungsprufung", "DB-Verbindungspruefung", "DB-Verbindung"],
                        ["DB-Verbindungspruefung"]),
                    ["DB-Erstellung / DB-Vorbereitung"] = new(
                        ["MigrateAsync", "DatabaseInitialized", "DB-Erstellung", "DB-Vorbereitung"],
                        ["DB-Erstellung", "DB-Vorbereitung"],
                        ["DB-Erstellung / DB-Vorbereitung"]),
                    ["Initiale Migrationen im Setup-Kontext"] = new(
                        ["GetPendingMigrationsAsync", "MigrationApplied", "MigrateAsync"],
                        ["Migrationsausfuhrung", "Initial-Migration", "Migrationen"],
                        ["Initiale Migrationen im Setup-Kontext"]),
                    ["Technische Erstinitialisierung"] = new(
                        ["EnsureRoleAsync", "EnsureRootOrganizationUnitAsync", "EnsureLocalCredentialAsync"],
                        ["technische Erstinitialisierung"],
                        ["Technische Erstinitialisierung"]),
                    ["Setup-Abschlusslogik"] = new(
                        ["MarkCompleted", "MarkFailed", "CompletedAtUtc"],
                        ["Setup-Abschlusslogik"],
                        ["Setup-Abschlusslogik"])
                }),
            ["M801"] = new ModuleAuditProfile(
                new Dictionary<string, ModuleAuditEvidenceProfile>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Sicherheitsgrundlagen"] = new(
                        ["AccessRule", "SecurityErrorCodes"],
                        ["Sicherheitsgrundlage", "Sicherheitsgrundsatze"],
                        ["Sicherheitsgrundlagen"]),
                    ["Basiskonzepte"] = new(
                        ["AccessTargetType", "AccessSubjectType", "AccessRuleMode"],
                        ["Basiskonzepte", "Baseline-Sicherheitsanforderungen"],
                        ["Basiskonzepte"]),
                    ["Ubergreifende Sicherheitsregeln"] = new(
                        ["VisibilityRules", "AccessRuleRepositories"],
                        ["systemweite Security-Standards", "Sicherheitsarchitektur-Richtlinien"],
                        ["Ubergreifende Sicherheitsregeln"]),
                    ["Sicherheitsprinzipien fuer alle Module"] = new(
                        ["SecurityErrorCodes", "Unauthorized", "Forbidden"],
                        ["Verbindliche Prinzipien", "jeder Zugriff muss gepruft werden"],
                        ["Sicherheitsprinzipien fuer alle Module"]),
                    ["Systemweite Gueltigkeit"] = new(
                        ["AccessRuleRepositories", "VisibilityRules"],
                        ["systemweite Security-Standards", "Grundlage fur alle Security-Module"],
                        ["Systemweite Gueltigkeit"]),
                    ["Keine modulbezogene Fachlogik"] = new(
                        ["AccessRule", "SecurityErrorCodes"],
                        ["keine operative Logik", "keine Laufzeitentscheidungen"],
                        ["Keine modulbezogene Fachlogik"])
                }),
            ["M802"] = new ModuleAuditProfile(
                new Dictionary<string, ModuleAuditEvidenceProfile>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Schutz gegen typische Web-/App-Angriffe"] = new(
                        ["ValidateRequestAsync", "OpenIdConnect", "CookieSecurePolicy"],
                        ["schutzt die Anwendung selbst vor Angriffen", "Injection", "XSS", "CSRF"],
                        ["Schutz gegen typische Web-/App-Angriffe"]),
                    ["Eingabeabsicherung"] = new(
                        ["ValidationStep", "ICommandValidator", "ValidationFailed"],
                        ["jede Eingabe muss validiert werden", "Input Validation verpflichtend"],
                        ["Eingabeabsicherung"]),
                    ["Sichere Request-/Response-Verarbeitung"] = new(
                        ["HttpOnly", "SameSite", "Secure", "SecurePolicy"],
                        ["API-Endpunkte mussen abgesichert sein", "sichere Fehlerbehandlung"],
                        ["Sichere Request-/Response-Verarbeitung"]),
                    ["Technische Autorisierungsdurchsetzung auf Anwendungsebene"] = new(
                        ["AuthorizationService", "AuthorizationStep", "IExecutionPipeline"],
                        ["API-Endpunkte mussen abgesichert sein", "keine Authentifizierung"],
                        ["Technische Autorisierungsdurchsetzung auf Anwendungsebene"]),
                    ["Schutz vor Injection"] = new(
                        ["ValidationStep", "ICommandValidator", "MapInboundClaims = false"],
                        ["Injection", "Input Validation verpflichtend"],
                        ["Schutz vor Injection"]),
                    ["Schutz vor Manipulation"] = new(
                        ["ValidateRequestAsync", "state", "nonce"],
                        ["CSRF", "Fremdclaims"],
                        ["Schutz vor Manipulation"]),
                    ["Schutz vor unsicheren Eingaben"] = new(
                        ["ValidationStep", "ValidationFailed", "AgainstNullOrWhiteSpace"],
                        ["unsichere Eingaben", "keine direkte Verarbeitung unvalidierter Daten"],
                        ["Schutz vor unsicheren Eingaben"]),
                    ["Keine Umgehung von Sicherheitspruefungen"] = new(
                        ["AuthorizationStep", "ValidationStep", "IExecutionPipeline"],
                        ["keine Umgehung", "jede Eingabe muss validiert werden"],
                        ["Keine Umgehung von Sicherheitspruefungen"])
                })
        };

    public static bool TryGet(string moduleCode, out ModuleAuditProfile profile)
    {
        return Profiles.TryGetValue(moduleCode, out profile!);
    }
}
