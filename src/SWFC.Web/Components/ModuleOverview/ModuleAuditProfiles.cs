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
            ["M501"] = BuildProfile(
                [
                    "ZeitplÃƒÂ¤ne",
                    "Periodische Trigger",
                    "Terminbasierte AuslÃƒÂ¶sung",
                    "FÃƒÂ¤lligkeitsauslÃƒÂ¶sung",
                    "Geplante Runtime-Starts",
                    "WartungsfÃƒÂ¤lligkeiten (M202)",
                    "PrÃƒÂ¼fzyklen (M203)",
                    "Geplante Jobs",
                    "Periodische Auswertungen",
                    "Automatische KontrolllÃƒÂ¤ufe",
                    "Scheduler lÃƒÂ¶st aus, entscheidet aber nicht fachlich"
                ],
                ["RuntimeScheduler", "SchedulerJobSchedule", "ScheduledRuntimeTrigger"],
                ["M501"],
                ["M501"]),
            ["M502"] = BuildProfile(
                [
                    "Automatisierungsregeln",
                    "Bedingung Ã¢â€ â€™ Reaktion",
                    "Definierte Ablaufketten",
                    "Automatische Systemreaktionen",
                    "FÃƒÂ¤llige Wartung erzeugt Auftrag",
                    "Negativer PrÃƒÂ¼fstatus erzeugt Folgeprozess",
                    "Mindestbestand erzeugt Einkaufsbedarf",
                    "Ereignis erzeugt Benachrichtigung",
                    "Keine automatische Umgehung von Sicherheitsregeln",
                    "Keine automatische kritische Steuerung ohne Freigabemechanismen",
                    "Automatisierung auditierbar halten"
                ],
                ["AutomationRuleEngine", "AutomationRule", "AutomationDecision"],
                ["M502"],
                ["M502"]),
            ["M503"] = BuildProfile(
                [
                    "AusfÃƒÂ¼hrung geplanter Jobs",
                    "Kontrollierte ProzessausfÃƒÂ¼hrung",
                    "Wiederholungslogik",
                    "AusfÃƒÂ¼hrungsstatus",
                    "Fehler- und Wiederanlaufverhalten",
                    "Geplante Wartungsjobs",
                    "PrÃƒÂ¼fzyklus-AusfÃƒÂ¼hrung",
                    "Periodische Synchronisationen",
                    "Automatisierte HintergrundlÃƒÂ¤ufe",
                    "Fehler protokollieren",
                    "Keine Umgehung von Sicherheits- oder Fachregeln"
                ],
                ["RuntimeJobExecutor", "RuntimeJobRun", "RetryAsync"],
                ["M503"],
                ["M503"]),
            ["M504"] = BuildProfile(
                [
                    "Live-Ansichten",
                    "LeitwartenoberflÃƒÂ¤chen",
                    "ÃƒÅ“bersicht mehrerer Anlagen / Bereiche",
                    "Bedien- und EingriffsoberflÃƒÂ¤chen",
                    "Zentrale Kontrollansichten",
                    "Schaltwarte / Leitwarte",
                    "ÃƒÅ“berwachung laufender Anlagen",
                    "Anzeige von ZustÃƒÂ¤nden",
                    "Kontrollierte Eingriffe",
                    "SpÃƒÂ¤tere Spiegelung / Steuerung von Leitwartenbildschirmen",
                    "Berechtigung vorhanden",
                    "SicherheitsprÃƒÂ¼fung",
                    "Definierte Freigaberegeln",
                    "Abstimmung mit Personal vor Ort",
                    "Protokollierung ÃƒÂ¼ber M805",
                    "Keine automatische Blindsteuerung",
                    "Maschine / SPS behÃƒÂ¤lt Sicherheitsoberhand",
                    "Keine Ersetzung physischer Sicherheitsmechanismen"
                ],
                ["ControlDeskRuntime", "ControlCommandDecision", "ControlDeskSnapshot"],
                ["M504"],
                ["M504"]),
            ["M505"] = BuildProfile(
                [
                    "Echtzeitdatenverarbeitung",
                    "Stream-/Event-Aggregation",
                    "Live-Zustandsbildung",
                    "Reaktion auf eingehende Runtime-Daten",
                    "MaschinenzustÃƒÂ¤nde laufend aktualisieren",
                    "Messwerte in Echtzeit verarbeiten",
                    "Ereignisse aggregieren",
                    "Live-Status fÃƒÂ¼r Leitwarte bereitstellen",
                    "Zusammenarbeit mit M404",
                    "Zusammenarbeit mit M405",
                    "Zusammenarbeit mit M504",
                    "Zusammenarbeit mit M212",
                    "Zusammenarbeit mit M205",
                    "Keine Sicherheitsumgehung",
                    "Keine fachliche Stammdatenhaltung"
                ],
                ["RealTimeProcessor", "RealTimeStateSnapshot", "RealTimeEnvelope"],
                ["M505"],
                ["M505"]),
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
                }),
            ["M803"] = BuildProfile(
                [
                    "VerschlÃƒÂ¼sselung",
                    "Zugriffsschutz auf Datenebene",
                    "Schutz sensibler Informationen",
                    "Sichere Speicherung",
                    "Besonderer Schutz sensibler Daten",
                    "Zugriff nach Sicherheits- und Berechtigungsregeln"
                ],
                ["DataProtectionService", "ProtectedDataPayload", "ISensitiveDataProtector", "data.protect", "data.reveal"],
                ["M803", "Data Security", "sensible Daten", "Zugriffskontrolle"],
                ["M803_DataProtection"]),
            ["M804"] = BuildProfile(
                [
                    "CI/CD-Sicherheitsregeln",
                    "Pipeline-Sicherheit",
                    "SicherheitsprÃƒÂ¼fungen im Entwicklungsprozess",
                    "Sichere Bereitstellung",
                    "SicherheitsprÃƒÂ¼fungen als Teil des Entwicklungsprozesses",
                    "Kein unsicheres Deployment"
                ],
                ["SecurityReleaseGate", "SecurityReleaseEvidence", "BuildSigned", "SecurityTestsPassed"],
                ["M804", "DevSecOps", "sichere Build-Prozesse", "Sicherheitschecks"],
                ["M804_ReleaseGate"]),
            ["M805"] = BuildProfile(
                [
                    "AuditLog",
                    "AuditService",
                    "Protokollierung sicherheitsrelevanter Ereignisse",
                    "Nachvollziehbarkeit von Ãƒâ€žnderungen",
                    "Nachvollziehbarkeit von Aktionen",
                    "Keine LÃƒÂ¶schung auditrelevanter Daten",
                    "Wer / wann / was / vorher / nachher nachvollziehbar machen"
                ],
                ["AuditLog", "AuditService", "AuditWriteRequest", "ToAuditWriteRequest"],
                ["M805", "Audit Logging", "wer", "wann", "was"],
                ["M805_Audit", "Audit_Service"]),
            ["M806"] = BuildProfile(
                [
                    "RBAC",
                    "ABAC",
                    "Kontextregeln",
                    "Allow / Deny",
                    "Vererbung",
                    "Sichtbarkeit vs. Aktion",
                    "Freigaben fÃƒÂ¼r sicherheitskritische Aktionen",
                    "Zentrale Auswertung von Berechtigungen",
                    "Keine verteilte Schattenlogik",
                    "Zugriff und Aktion getrennt bewerten"
                ],
                ["AccessDecisionService", "AccessDecisionRequest", "RequiredAttributes", "RequiresApproval", "MatchesWildcardPermission"],
                ["M806", "Access Control", "RBAC", "ABAC"],
                ["M806_Should", "M806_Critical_Actions"]),
            ["M807"] = BuildProfile(
                [
                    "API Keys",
                    "Tokens",
                    "Zertifikate",
                    "VerschlÃƒÂ¼sselungsschlÃƒÂ¼ssel",
                    "Sichere Ablage",
                    "Sichere Nutzung",
                    "Keine Secrets im Code",
                    "Kein unkontrollierter Zugriff",
                    "Nutzung nachvollziehbar und abgesichert"
                ],
                ["SecretVaultService", "SecretKind", "ISecretVaultRepository", "security.secrets.write", "SecretRotated"],
                ["M807", "Secrets", "API Keys", "Tokens"],
                ["M807_Secrets"]),
            ["M808"] = BuildProfile(
                [
                    "Erkennung ungewÃƒÂ¶hnlicher Zugriffe",
                    "Erkennung verdÃƒÂ¤chtiger Muster",
                    "Sicherheitsalarme",
                    "Monitoring sicherheitsrelevanter Ereignisse",
                    "Viele Fehlanmeldungen",
                    "UngewÃƒÂ¶hnliche API-Zugriffe",
                    "AuffÃƒÂ¤llige Nutzungsmuster",
                    "VerdÃƒÂ¤chtige AktivitÃƒÂ¤ten im System",
                    "Nicht nur loggen, sondern aktiv ÃƒÂ¼berwachen",
                    "Kritische AuffÃƒÂ¤lligkeiten erzeugen Reaktion / Alarm"
                ],
                ["SecurityMonitoringService", "SecurityAlert", "LoginFailed", "SecurityAlertRaised"],
                ["M808", "Security Monitoring", "Alarme", "sicherheitsrelevante Muster"],
                ["M808_Monitoring"]),
            ["M809"] = BuildProfile(
                [
                    "Sicherheitsrichtlinien",
                    "Zugriffsrichtlinien",
                    "Policy-Vorgaben",
                    "Compliance-Regeln",
                    "Nachweisbare Regelwerke",
                    "Policies sind verbindlich",
                    "Abweichungen nachvollziehbar und begrÃƒÂ¼ndet machen",
                    "Policies wirken auf Konfiguration, Zugriff und Prozesse"
                ],
                ["SecurityPolicyService", "SecurityPolicy", "PolicyViolation", "security.policies.write"],
                ["M809", "Compliance", "Policies", "Policy"],
                ["M809_Policies"])
        };

    private static ModuleAuditProfile BuildProfile(
        IReadOnlyList<string> workItemTitles,
        IReadOnlyList<string> codeTerms,
        IReadOnlyList<string> documentationTerms,
        IReadOnlyList<string> testTerms)
    {
        return new ModuleAuditProfile(
            workItemTitles.ToDictionary(
                title => title,
                _ => new ModuleAuditEvidenceProfile(codeTerms, documentationTerms, testTerms),
                StringComparer.OrdinalIgnoreCase));
    }

    public static bool TryGet(string moduleCode, out ModuleAuditProfile profile)
    {
        return Profiles.TryGetValue(moduleCode, out profile!);
    }
}
