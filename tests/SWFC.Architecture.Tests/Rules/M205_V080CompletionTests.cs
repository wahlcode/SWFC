using System.Text.Json;
using SWFC.Architecture.Tests.Support;
using SWFC.Application.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Application.M200_Business.M205_Energy.EnergyReadings;
using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M200_Business.M205_Energy.EnergyMeters;
using SWFC.Domain.M200_Business.M205_Energy.EnergyReadings;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M205_V080CompletionTests
{
    [Fact]
    public void V080_Roadmap_Should_Be_Marked_Done_With_All_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.8.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.Contains("M205", ReadStringArray(version, "PrimaryModules"));
        Assert.All(
            new[] { "M201", "M102", "M805" },
            required => Assert.Contains(required, ReadStringArray(version, "RequiredModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M205_WorkItems_Should_Be_Done_For_V080()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var m205 = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Single(module => module.GetProperty("Code").GetString() == "M205");

        Assert.Equal("Full Complete", m205.GetProperty("Status").GetString());
        Assert.Equal(100, m205.GetProperty("ProgressPercent").GetInt32());
        Assert.All(
            m205.GetProperty("WorkItems").EnumerateArray(),
            item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M205_Domain_Should_Capture_Extensible_Media_Hierarchy_Offline_Rfid_And_Plausibility()
    {
        var context = ChangeContext.Create("tester", "v0.8 verification");
        var parentMeterId = Guid.NewGuid();
        var machineId = Guid.NewGuid();

        var meter = EnergyMeter.Create(
            new EnergyMeterName("Plant compressed air"),
            EnergyMediumType.Other,
            new EnergyMediumName("Compressed air"),
            new EnergyMeterUnit("m3"),
            isManualEntryEnabled: true,
            isExternalImportEnabled: true,
            EnergyExternalSystem.CreateOptional("M404-IoT"),
            EnergyMeterRfidTag.CreateOptional("RFID-001"),
            supportsOfflineCapture: true,
            parentMeterId,
            machineId,
            context);

        var offlineCaptureId = Guid.NewGuid();
        var reading = EnergyReading.Create(
            meter.Id,
            new EnergyReadingDate(new DateTime(2026, 4, 26, 10, 0, 0, DateTimeKind.Utc)),
            new EnergyReadingValue(1234.5678m),
            EnergyReadingSource.Manual,
            "operator-1",
            EnergyReadingCaptureContext.CreateOptional("mobile offline round"),
            EnergyReadingRfidTag.CreateOptional("RFID-001"),
            null,
            offlineCaptureId,
            new DateTime(2026, 4, 26, 10, 1, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 26, 10, 30, 0, DateTimeKind.Utc),
            EnergyReadingPlausibilityStatus.Flagged,
            EnergyReadingPlausibilityNote.CreateOptional("Manual value differs from automatic control value."),
            context);

        Assert.Equal("Compressed air", meter.MediumName.Value);
        Assert.Equal(parentMeterId, meter.ParentMeterId);
        Assert.Equal(machineId, meter.MachineId);
        Assert.True(meter.SupportsOfflineCapture);
        Assert.Equal(1234.568m, reading.Value.Value);
        Assert.Equal(offlineCaptureId, reading.OfflineCaptureId);
        Assert.True(reading.IsOfflineCapture);
        Assert.True(reading.IsPlausibilityFlagged);
        Assert.Equal("RFID-001", reading.RfidTag?.Value);
    }

    [Fact]
    public async Task M205_Validators_Should_Require_Rfid_Or_Exception_For_Manual_Offline_And_Flagged_Readings()
    {
        var validator = new CreateEnergyReadingValidator();

        var invalidManual = await validator.ValidateAsync(new CreateEnergyReadingCommand(
            Guid.NewGuid(),
            DateTime.UtcNow,
            1,
            EnergyReadingSource.Manual,
            "operator-1",
            null,
            null,
            null,
            null,
            null,
            null,
            EnergyReadingPlausibilityStatus.Normal,
            null,
            "reason"));

        Assert.False(invalidManual.IsValid);

        var validException = await validator.ValidateAsync(new CreateEnergyReadingCommand(
            Guid.NewGuid(),
            DateTime.UtcNow,
            1,
            EnergyReadingSource.Manual,
            "operator-1",
            "mobile route",
            null,
            "RFID unreadable",
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow,
            EnergyReadingPlausibilityStatus.Flagged,
            "Outlier retained for audit.",
            "reason"));

        Assert.True(validException.IsValid);
    }

    [Fact]
    public void M205_Integration_Should_Have_Persistence_Ui_Analysis_And_No_Delete_Use_Cases()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Configurations", "M200-Business", "M205-Energy", "EnergyMeterConfiguration.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Configurations", "M200-Business", "M205-Energy", "EnergyReadingConfiguration.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Migrations", "20260426080000_M205_CompleteEnergyCapture.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "DependencyInjection", "ServiceCollectionExtensions.cs"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M205-Energy", "Energy.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M205-Energy", "EnergyMeterDetail.razor"),
            RepositoryRoot.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M205-Energy", "EnergyReadingDetail.razor"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M205-Energy", "Analysis", "EnergyAnalysisDtos.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M205-Energy", "Analysis", "GetEnergyAnalysis.cs")
        };

        Assert.All(requiredFiles, file => Assert.True(File.Exists(file), file));

        var combinedContent = string.Join(Environment.NewLine, requiredFiles.Select(File.ReadAllText));
        Assert.Contains("MediumName", combinedContent, StringComparison.Ordinal);
        Assert.Contains("ParentMeterId", combinedContent, StringComparison.Ordinal);
        Assert.Contains("MachineId", combinedContent, StringComparison.Ordinal);
        Assert.Contains("RfidExceptionReason", combinedContent, StringComparison.Ordinal);
        Assert.Contains("OfflineCaptureId", combinedContent, StringComparison.Ordinal);
        Assert.Contains("PlausibilityStatus", combinedContent, StringComparison.Ordinal);
        Assert.Contains("CaptureComparisons", combinedContent, StringComparison.Ordinal);
        Assert.Contains("AddScoped<IEnergyMeterReadRepository", combinedContent, StringComparison.Ordinal);

        var deleteUseCases = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Application", "M200-Business", "M205-Energy"),
                "Delete*.cs",
                includeGeneratedFiles: false)
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(deleteUseCases.Length == 0, $"M205 must not delete readings: {string.Join(", ", deleteUseCases)}");
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();
}
