using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using SWFC.Architecture.Tests.Support;
using SWFC.Web.Components.ModuleOverview;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M101_M103_M107_M801_M802_AuditVerificationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void V010_Status_Data_Should_Mark_All_Foundation_Modules_And_Roadmap_As_Completed()
    {
        var modulesPath = RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json");
        var roadmapPath = RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json");

        using var modulesDocument = JsonDocument.Parse(File.ReadAllText(modulesPath));
        using var roadmapDocument = JsonDocument.Parse(File.ReadAllText(roadmapPath));

        foreach (var moduleCode in new[] { "M101", "M103", "M107", "M801", "M802" })
        {
            var module = modulesDocument.RootElement
                .GetProperty("Groups")
                .EnumerateArray()
                .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
                .First(item => string.Equals(item.GetProperty("Code").GetString(), moduleCode, StringComparison.OrdinalIgnoreCase));

            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                workItem => Assert.Equal("Done", workItem.GetProperty("Status").GetString()));
        }

        var v010 = roadmapDocument.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .First(item => string.Equals(item.GetProperty("Version").GetString(), "v0.1.0", StringComparison.OrdinalIgnoreCase));

        Assert.Equal("Done", v010.GetProperty("Status").GetString());
        Assert.All(
            v010.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public async Task V010_Module_Audit_Should_Verify_All_Foundation_Modules()
    {
        var environment = new TestWebHostEnvironment
        {
            ApplicationName = "SWFC.Web",
            ContentRootPath = RepositoryRoot.Combine("src", "SWFC.Web"),
            WebRootPath = RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot"),
            ContentRootFileProvider = new PhysicalFileProvider(RepositoryRoot.Combine("src", "SWFC.Web")),
            WebRootFileProvider = new PhysicalFileProvider(RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot"))
        };
        var auditService = new ModuleAuditService(environment);
        var modulesPath = RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json");
        var file = JsonSerializer.Deserialize<ModuleOverviewFileDto>(File.ReadAllText(modulesPath), JsonOptions)
            ?? throw new InvalidOperationException("modules.json could not be deserialized.");

        var results = await auditService.GenerateAuditResultsAsync(file.Groups);

        foreach (var moduleCode in new[] { "M101", "M103", "M107", "M801", "M802" })
        {
            Assert.True(results.TryGetValue(moduleCode, out var result), $"Missing audit result for {moduleCode}.");
            Assert.Equal(ModuleAuditStatuses.Verified, result.Status);
            Assert.Equal(100, result.ScorePercent);
            Assert.Equal(ModuleAuditPartStatuses.Complete, result.CodeStatus);
            Assert.Equal(ModuleAuditPartStatuses.Complete, result.MdStatus);
            Assert.Equal(ModuleAuditPartStatuses.Complete, result.TestStatus);
            Assert.All(result.WorkItems, item => Assert.Equal(ModuleWorkItemStatuses.Done, item.Status));
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
