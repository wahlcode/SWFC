using SWFC.Web.Models;

namespace SWFC.Web.Data;

public static class ModuleOverviewData
{
    public static List<ModuleOverviewItem> GetModules() =>
    [
        new()
        {
            GroupCode = "M100",
            GroupTitle = "System",
            Code = "M101",
            Title = "Foundation",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Result / Result", Status = "Done", Level = "Core" },
                new() { Title = "Error Handling", Status = "Done", Level = "Core" },
                new() { Title = "Guards", Status = "Done", Level = "Core" },
                new() { Title = "ValueObjects", Status = "Done", Level = "Core" }
            ]
        },
        new()
        {
            GroupCode = "M100",
            GroupTitle = "System",
            Code = "M102",
            Title = "Organization",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Standorte", Status = "Done", Level = "Core" },
                new() { Title = "Abteilungen", Status = "Done", Level = "Core" },
                new() { Title = "Kostenstellen", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Bereichslogik", Status = "Open", Level = "OptionalCore" }
            ]
        },
        new()
        {
            GroupCode = "M100",
            GroupTitle = "System",
            Code = "M103",
            Title = "Authentication",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "CurrentUserService", Status = "Done", Level = "Core" },
                new() { Title = "Auth-Modus Umschaltung", Status = "Done", Level = "Core" },
                new() { Title = "Local Provider", Status = "Done", Level = "OptionalCore" },
                new() { Title = "SSO Provider", Status = "Done", Level = "Core" },
                new() { Title = "Rollen / Claims", Status = "Done", Level = "Core" }
            ]
        },
        new()
        {
            GroupCode = "M100",
            GroupTitle = "System",
            Code = "M104",
            Title = "Documents",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "Dokumente", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Upload", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Versionierung", Status = "Open", Level = "Extension" },
                new() { Title = "externe Ablage (DMS)", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M100",
            GroupTitle = "System",
            Code = "M105",
            Title = "Configuration",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Systemeinstellungen", Status = "Open", Level = "Core" },
                new() { Title = "Feature Flags", Status = "Open", Level = "Core" },
                new() { Title = "Environment Settings", Status = "Open", Level = "Core" }
            ]
        },
        new()
        {
            GroupCode = "M100",
            GroupTitle = "System",
            Code = "M106",
            Title = "Theme",
            Status = "InProgress",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "CSS Theme", Status = "Done", Level = "Core" },
                new() { Title = "Statusfarben", Status = "Done", Level = "Core" },
                new() { Title = "Layout Tokens", Status = "Done", Level = "Core" },
                new() { Title = "Dark Mode", Status = "Open", Level = "Extension" },
                new() { Title = "Benutzer-Theme", Status = "Open", Level = "Extension" }
            ]
        },

        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M201",
            Title = "Assets",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Create / Update / Delete", Status = "Done", Level = "Core" },
                new() { Title = "Stammdaten", Status = "Done", Level = "Core" },
                new() { Title = "Status", Status = "Done", Level = "Core" },
                new() { Title = "Erweiterte Felder", Status = "Open", Level = "Extension" },
                new() { Title = "Asset-Typen", Status = "Open", Level = "OptionalCore" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M202",
            Title = "Maintenance",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Wartungspläne", Status = "Open", Level = "Core" },
                new() { Title = "Wartungsaufträge", Status = "Open", Level = "Core" },
                new() { Title = "Wartungshistorie", Status = "Open", Level = "Core" },
                new() { Title = "automatische Wartung (Scheduler)", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M203",
            Title = "Inspections",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Prüfpläne", Status = "Open", Level = "Core" },
                new() { Title = "Prüfungen", Status = "Open", Level = "Core" },
                new() { Title = "Prüfergebnisse", Status = "Open", Level = "Core" },
                new() { Title = "automatische Prüfzyklen", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M204",
            Title = "Inventory",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Artikel", Status = "Open", Level = "Core" },
                new() { Title = "Lager", Status = "Open", Level = "Core" },
                new() { Title = "Lagerorte", Status = "Open", Level = "Core" },
                new() { Title = "Bestände", Status = "Open", Level = "Core" },
                new() { Title = "Bewegungen", Status = "Open", Level = "Core" },
                new() { Title = "Buchungen (Anlage/Projekt)", Status = "Open", Level = "Core" },
                new() { Title = "Barcode / Scanner", Status = "Open", Level = "Extension" },
                new() { Title = "externe Lagerintegration", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M205",
            Title = "Energy",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "Verbrauchsdaten", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Messwerte", Status = "Open", Level = "OptionalCore" },
                new() { Title = "IoT-Anbindung", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M206",
            Title = "Purchasing",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Bestellungen", Status = "Open", Level = "Core" },
                new() { Title = "Lieferanten", Status = "Open", Level = "Core" },
                new() { Title = "Wareneingang", Status = "Open", Level = "Core" },
                new() { Title = "externe ERP-Anbindung", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M207",
            Title = "Quality",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Qualitätsmeldungen", Status = "Open", Level = "Core" },
                new() { Title = "Abweichungen", Status = "Open", Level = "Core" },
                new() { Title = "Prüfstatus", Status = "Open", Level = "Core" },
                new() { Title = "externe QM-Systeme", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M208",
            Title = "Safety",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Sicherheitsdaten", Status = "Open", Level = "Core" },
                new() { Title = "Gefährdungen", Status = "Open", Level = "Core" },
                new() { Title = "Compliance-Integrationen", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M209",
            Title = "Projects",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Projekte", Status = "Open", Level = "Core" },
                new() { Title = "Maßnahmen", Status = "Open", Level = "Core" },
                new() { Title = "Materialbuchung", Status = "Open", Level = "Core" },
                new() { Title = "externe Projekttools", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M210",
            Title = "Customers",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "Kunden", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Anlagenzuordnung", Status = "Open", Level = "OptionalCore" },
                new() { Title = "CRM-Anbindung", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M200",
            GroupTitle = "Business",
            Code = "M211",
            Title = "Analytics",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "KPIs", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Reports", Status = "Open", Level = "OptionalCore" },
                new() { Title = "BI-Anbindung", Status = "Open", Level = "Extension" }
            ]
        },

        new()
        {
            GroupCode = "M300",
            GroupTitle = "Presentation",
            Code = "M301",
            Title = "Persistence",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "DbContext", Status = "Done", Level = "Core" },
                new() { Title = "Mapping", Status = "Done", Level = "Core" },
                new() { Title = "Migrationen", Status = "Done", Level = "Core" }
            ]
        },
        new()
        {
            GroupCode = "M300",
            GroupTitle = "Presentation",
            Code = "M302",
            Title = "Reporting",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "Reports", Status = "Open", Level = "OptionalCore" },
                new() { Title = "PDF Export", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Excel Export", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M300",
            GroupTitle = "Presentation",
            Code = "M303",
            Title = "Notifications",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "Systemmeldungen", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Alerts", Status = "Open", Level = "OptionalCore" },
                new() { Title = "E-Mail", Status = "Open", Level = "Extension" },
                new() { Title = "Teams / Slack", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M300",
            GroupTitle = "Presentation",
            Code = "M304",
            Title = "Search",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "Suche", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Filter", Status = "Open", Level = "OptionalCore" },
                new() { Title = "Elastic / externe Suche", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M300",
            GroupTitle = "Presentation",
            Code = "M305",
            Title = "AppShell",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Layout", Status = "Done", Level = "Core" },
                new() { Title = "Navigation", Status = "Done", Level = "Core" },
                new() { Title = "Sidebar", Status = "Done", Level = "Core" }
            ]
        },

        new()
        {
            GroupCode = "M400",
            GroupTitle = "Integrations",
            Code = "M401",
            Title = "ImportExport",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "CSV", Status = "Open", Level = "Extension" },
                new() { Title = "Excel", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M400",
            GroupTitle = "Integrations",
            Code = "M402",
            Title = "API",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "REST API", Status = "Open", Level = "Extension" },
                new() { Title = "externe Zugriffe", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M400",
            GroupTitle = "Integrations",
            Code = "M403",
            Title = "ERP",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "SAP / andere Systeme", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M400",
            GroupTitle = "Integrations",
            Code = "M404",
            Title = "IoT",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Sensoren", Status = "Open", Level = "Extension" },
                new() { Title = "Maschinenanbindung", Status = "Open", Level = "Extension" }
            ]
        },

        new()
        {
            GroupCode = "M500",
            GroupTitle = "Runtime",
            Code = "M501",
            Title = "Scheduler",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Jobs", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M500",
            GroupTitle = "Runtime",
            Code = "M502",
            Title = "Communication",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Messaging", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M500",
            GroupTitle = "Runtime",
            Code = "M503",
            Title = "Automation",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Regeln", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M500",
            GroupTitle = "Runtime",
            Code = "M504",
            Title = "Updater",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Updates", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M500",
            GroupTitle = "Runtime",
            Code = "M505",
            Title = "Versioning",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Versionshistorie", Status = "Open", Level = "Extension" }
            ]
        },

        new()
        {
            GroupCode = "M600",
            GroupTitle = "Planning",
            Code = "M601",
            Title = "Roadmap",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "Roadmap", Status = "Open", Level = "OptionalCore" }
            ]
        },
        new()
        {
            GroupCode = "M600",
            GroupTitle = "Planning",
            Code = "M602",
            Title = "Concepts",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "Concepts", Status = "Open", Level = "OptionalCore" }
            ]
        },
        new()
        {
            GroupCode = "M600",
            GroupTitle = "Planning",
            Code = "M603",
            Title = "Prototypes",
            Status = "Open",
            Level = "OptionalCore",
            WorkItems =
            [
                new() { Title = "Prototypes", Status = "Open", Level = "OptionalCore" }
            ]
        },

        new()
        {
            GroupCode = "M700",
            GroupTitle = "Support",
            Code = "M701",
            Title = "BugTracking",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "BugTracking", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M700",
            GroupTitle = "Support",
            Code = "M702",
            Title = "ChangeRequests",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "ChangeRequests", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M700",
            GroupTitle = "Support",
            Code = "M703",
            Title = "SupportCases",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "SupportCases", Status = "Open", Level = "Extension" }
            ]
        },

        new()
        {
            GroupCode = "M800",
            GroupTitle = "Security",
            Code = "M801",
            Title = "Security Foundation",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Regeln", Status = "Done", Level = "Core" }
            ]
        },
        new()
        {
            GroupCode = "M800",
            GroupTitle = "Security",
            Code = "M802",
            Title = "Application Security",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Pipeline", Status = "Done", Level = "Core" },
                new() { Title = "Authorization", Status = "Done", Level = "Core" }
            ]
        },
        new()
        {
            GroupCode = "M800",
            GroupTitle = "Security",
            Code = "M803",
            Title = "Data Security",
            Status = "Open",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Zugriffsschutz", Status = "Open", Level = "Core" },
                new() { Title = "Verschlüsselung", Status = "Open", Level = "Core" }
            ]
        },
        new()
        {
            GroupCode = "M800",
            GroupTitle = "Security",
            Code = "M804",
            Title = "DevSecOps",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "CI/CD", Status = "Done", Level = "Core" }
            ]
        },
        new()
        {
            GroupCode = "M800",
            GroupTitle = "Security",
            Code = "M805",
            Title = "Audit Logging",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "AuditLog", Status = "Done", Level = "Core" },
                new() { Title = "AuditService", Status = "Done", Level = "Core" }
            ]
        },

        new()
        {
            GroupCode = "M900",
            GroupTitle = "Intelligence",
            Code = "M900",
            Title = "Architekturprüfung",
            Status = "Done",
            Level = "Core",
            WorkItems =
            [
                new() { Title = "Architekturprüfung", Status = "Done", Level = "Core" }
            ]
        },
        new()
        {
            GroupCode = "M900",
            GroupTitle = "Intelligence",
            Code = "M901",
            Title = "AI",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "AI", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M900",
            GroupTitle = "Intelligence",
            Code = "M902",
            Title = "Monitoring",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Monitoring", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M900",
            GroupTitle = "Intelligence",
            Code = "M903",
            Title = "Incident",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Incident", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M900",
            GroupTitle = "Intelligence",
            Code = "M904",
            Title = "Integration Governance",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Integration Governance", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M900",
            GroupTitle = "Intelligence",
            Code = "M905",
            Title = "Rule Engine",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Rule Engine", Status = "Open", Level = "Extension" }
            ]
        },

        new()
        {
            GroupCode = "M1000",
            GroupTitle = "Platform",
            Code = "M1001",
            Title = "Plugin System",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Plugin System", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M1000",
            GroupTitle = "Platform",
            Code = "M1002",
            Title = "Developer Platform",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Developer Platform", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M1000",
            GroupTitle = "Platform",
            Code = "M1003",
            Title = "Extension Management",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Extension Management", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M1000",
            GroupTitle = "Platform",
            Code = "M1004",
            Title = "Marketplace",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "Marketplace", Status = "Open", Level = "Extension" }
            ]
        },
        new()
        {
            GroupCode = "M1000",
            GroupTitle = "Platform",
            Code = "M1005",
            Title = "External Security",
            Status = "Open",
            Level = "Extension",
            WorkItems =
            [
                new() { Title = "External Security", Status = "Open", Level = "Extension" }
            ]
        }
    ];
}