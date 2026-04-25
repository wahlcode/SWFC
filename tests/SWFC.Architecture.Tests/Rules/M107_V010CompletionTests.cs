using SWFC.Architecture.Tests.Support;
using SWFC.Infrastructure.M100_System.M107_SetupDeployment.Entities;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M107_V010CompletionTests
{
    [Fact]
    public void M107_Setup_State_Bootstrap_Zustaende_And_Setup_Abschlusslogik_Should_Be_Tracked_Explicitly()
    {
        // Setup-State / Bootstrap-Zustaende
        // Setup-Abschlusslogik
        var startedAtUtc = DateTimeOffset.UtcNow;
        var setupState = SetupState.Create(startedAtUtc);

        setupState.MarkInProgress(startedAtUtc.AddMinutes(1));
        Assert.True(setupState.SetupInProgress);
        Assert.False(setupState.SetupCompleted);

        setupState.MarkCompleted(startedAtUtc.AddMinutes(2));
        Assert.True(setupState.IsConfigured);
        Assert.True(setupState.SetupCompleted);
        Assert.True(setupState.DatabaseInitialized);
        Assert.False(setupState.SetupInProgress);
        Assert.NotNull(setupState.CompletedAtUtc);

        setupState.MarkFailed(startedAtUtc.AddMinutes(3), "Connection lost");
        Assert.False(setupState.SetupInProgress);
        Assert.Equal("Connection lost", setupState.LastFailure);
    }

    [Fact]
    public void M107_Erstinstallation_Bootstrap_Kontext_DB_Verbindungspruefung_DB_Erstellung_DB_Vorbereitung_And_Initiale_Migrationen_Im_Setup_Kontext_Should_Have_Concrete_Artifacts()
    {
        // Erstinstallation / Bootstrap-Kontext
        // DB-Verbindungspruefung
        // DB-Erstellung / DB-Vorbereitung
        // Initiale Migrationen im Setup-Kontext
        // Technische Erstinitialisierung
        var initializerPath = RepositoryRoot.Combine("src", "SWFC.Infrastructure", "M100-System", "M107-Setup-Deployment", "M107SetupInitializer.cs");
        var appDbContextPath = RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Context", "AppDbContext.cs");
        var configurationPath = RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Configurations", "M100-System", "SetupStateConfiguration.cs");
        var migrationPath = RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Migrations", "20260424220605_M107SetupState.cs");

        var combinedContent = string.Join(
            Environment.NewLine,
            File.ReadAllText(initializerPath),
            File.ReadAllText(appDbContextPath),
            File.ReadAllText(configurationPath),
            File.ReadAllText(migrationPath));

        Assert.Contains("CanConnectAsync", combinedContent, StringComparison.Ordinal);
        Assert.Contains("MigrateAsync", combinedContent, StringComparison.Ordinal);
        Assert.Contains("GetPendingMigrationsAsync", combinedContent, StringComparison.Ordinal);
        Assert.Contains("EnsureSetupStateAsync", combinedContent, StringComparison.Ordinal);
        Assert.Contains("DbSet<SetupState>", combinedContent, StringComparison.Ordinal);
        Assert.Contains("SetupStateConfiguration", combinedContent, StringComparison.Ordinal);
        Assert.Contains("CreateTable(", combinedContent, StringComparison.Ordinal);
        Assert.Contains("EnsureRoleAsync", combinedContent, StringComparison.Ordinal);
        Assert.Contains("EnsureRootOrganizationUnitAsync", combinedContent, StringComparison.Ordinal);
    }
}
