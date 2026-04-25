using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class ModuleSpecificRuleTests
{
    [Fact]
    public void M201_Assets_Should_Expose_Documented_Core_Artifacts()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M201-Assets", "Machines", "Machine.cs"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M201-Assets", "MachineComponents", "MachineComponent.cs"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M201-Assets", "MachineComponentAreas", "MachineComponentArea.cs"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M201-Assets", "Hierarchy", "MachineHierarchyRules.cs"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M201-Assets", "MachineComponents", "MachineComponentHierarchyRules.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M201-Assets", "Machines", "CreateMachine.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M201-Assets", "MachineComponents", "CreateMachineComponent.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M201-Assets", "MachineComponentAreas", "CreateMachineComponentArea.cs")
        };

        var missingFiles = requiredFiles.Where(path => !File.Exists(path))
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(
            missingFiles.Length == 0,
            $"M201 documented core artifacts are missing: {string.Join(", ", missingFiles)}");
    }

    [Fact]
    public void M202_Maintenance_Should_Expose_Documented_Core_Artifacts_Without_Direct_Inventory_Coupling()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M202-Maintenance", "MaintenanceOrders", "MaintenanceOrder.cs"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M202-Maintenance", "MaintenancePlans", "MaintenancePlanModels.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M202-Maintenance", "MaintenanceOrders", "CreateMaintenanceOrder.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M202-Maintenance", "MaintenancePlans", "CreateMaintenancePlan.cs")
        };

        var missingFiles = requiredFiles.Where(path => !File.Exists(path))
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(
            missingFiles.Length == 0,
            $"M202 documented core artifacts are missing: {string.Join(", ", missingFiles)}");

        var forbiddenCouplings = RepositoryRoot.EnumerateFiles(System.IO.Path.Combine("src", "SWFC.Application", "M200-Business", "M202-Maintenance"), "*.cs", includeGeneratedFiles: false)
            .Concat(RepositoryRoot.EnumerateFiles(System.IO.Path.Combine("src", "SWFC.Domain", "M200-Business", "M202-Maintenance"), "*.cs", includeGeneratedFiles: false))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, index) => new
                {
                    RelativePath = RepositoryRoot.ToRelativePath(path),
                    LineNumber = index + 1,
                    Line = line
                }))
            .Where(entry => entry.Line.Contains("InventoryItem", StringComparison.Ordinal)
                || entry.Line.Contains("StockReservation", StringComparison.Ordinal)
                || entry.Line.Contains("StockMovement", StringComparison.Ordinal))
            .Select(entry => $"{entry.RelativePath}:{entry.LineNumber}")
            .ToArray();

        Assert.True(
            forbiddenCouplings.Length == 0,
            $"M202 should reference inventory only indirectly, but direct inventory coupling was found in: {string.Join(", ", forbiddenCouplings)}");
    }

    [Fact]
    public void M204_Inventory_Should_Expose_Stock_Movements_Instead_Of_Delete_Use_Cases_Or_Direct_Quantity_Setters()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M204-Inventory", "Stock", "CreateStockMovement.cs"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M204-Inventory", "Stock", "StockMovement.cs")
        };

        var missingFiles = requiredFiles.Where(path => !File.Exists(path))
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(
            missingFiles.Length == 0,
            $"M204 documented stock movement artifacts are missing: {string.Join(", ", missingFiles)}");

        var deleteUseCases = RepositoryRoot.EnumerateFiles(System.IO.Path.Combine("src", "SWFC.Application", "M200-Business", "M204-Inventory"), "Delete*.cs", includeGeneratedFiles: false)
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        var directQuantitySetters = RepositoryRoot.EnumerateFiles(System.IO.Path.Combine("src", "SWFC.Domain", "M200-Business", "M204-Inventory"), "*.cs", includeGeneratedFiles: false)
            .Where(path => File.ReadAllText(path).Contains("SetQuantity(", StringComparison.Ordinal))
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(
            deleteUseCases.Length == 0 && directQuantitySetters.Length == 0,
            $"M204 violations. Delete-oriented files: {string.Join(", ", deleteUseCases)}. Direct quantity setters: {string.Join(", ", directQuantitySetters)}");
    }

    [Fact]
    public void M205_Energy_Should_Expose_Documented_Core_Artifacts_And_Operational_Capture_Touchpoints()
    {
        var requiredFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M205-Energy", "EnergyMeters", "EnergyMeter.cs"),
            RepositoryRoot.Combine("src", "SWFC.Domain", "M200-Business", "M205-Energy", "EnergyReadings", "EnergyReading.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M205-Energy", "EnergyMeters", "CreateEnergyMeter.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M205-Energy", "EnergyReadings", "CreateEnergyReading.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M200-Business", "M205-Energy", "Analysis", "GetEnergyAnalysis.cs")
        };

        var missingFiles = requiredFiles.Where(path => !File.Exists(path))
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(
            missingFiles.Length == 0,
            $"M205 documented core artifacts are missing: {string.Join(", ", missingFiles)}");

        var energyFiles = RepositoryRoot.EnumerateFiles(System.IO.Path.Combine("src", "SWFC.Application", "M200-Business", "M205-Energy"), "*.cs", includeGeneratedFiles: false)
            .Concat(RepositoryRoot.EnumerateFiles(System.IO.Path.Combine("src", "SWFC.Domain", "M200-Business", "M205-Energy"), "*.cs", includeGeneratedFiles: false))
            .Concat(RepositoryRoot.EnumerateFiles(System.IO.Path.Combine("src", "SWFC.Web", "Pages", "M200-Business", "M205-Energy"), "*.razor", includeGeneratedFiles: false))
            .ToArray();

        var combinedContent = string.Join(
            Environment.NewLine,
            energyFiles.Select(File.ReadAllText));

        Assert.Contains("IsManualEntryEnabled", combinedContent, StringComparison.Ordinal);
        Assert.Contains("IsExternalImportEnabled", combinedContent, StringComparison.Ordinal);

        var missingTouchpoints = new List<string>();

        if (!combinedContent.Contains("RFID", StringComparison.OrdinalIgnoreCase))
        {
            missingTouchpoints.Add("RFID");
        }

        if (!combinedContent.Contains("Offline", StringComparison.OrdinalIgnoreCase))
        {
            missingTouchpoints.Add("Offline");
        }

        Assert.True(
            missingTouchpoints.Count == 0,
            $"M205 operational capture touchpoints missing from source: {string.Join(", ", missingTouchpoints)}");
    }

    [Fact]
    public void M504_Control_And_M805_Audit_Should_Have_Concrete_Runtime_And_Audit_Backing()
    {
        var hasControlModule = RepositoryRoot.EnumerateDirectories("src")
            .Any(path => System.IO.Path.GetFileName(path).StartsWith("M504-", StringComparison.OrdinalIgnoreCase));

        Assert.True(
            hasControlModule,
            "M504 is documented as a verbindliche control module, but no source directory starting with 'M504-' exists.");

        var requiredAuditFiles = new[]
        {
            RepositoryRoot.Combine("src", "SWFC.Domain", "M800-Security", "M805-Audit-Logging", "Entities", "AuditLog.cs"),
            RepositoryRoot.Combine("src", "SWFC.Application", "M800-Security", "M805-Audit-Logging", "Interfaces", "IAuditService.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "M800-Security", "Audit", "AuditService.cs"),
            RepositoryRoot.Combine("src", "SWFC.Infrastructure", "Persistence", "Repositories", "M800-Security", "AuditLogRepository.cs")
        };

        var missingAuditFiles = requiredAuditFiles.Where(path => !File.Exists(path))
            .Select(RepositoryRoot.ToRelativePath)
            .ToArray();

        Assert.True(
            missingAuditFiles.Length == 0,
            $"M805 documented audit artifacts are missing: {string.Join(", ", missingAuditFiles)}");
    }
}

