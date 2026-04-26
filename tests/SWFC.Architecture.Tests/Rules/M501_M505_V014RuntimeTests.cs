using System.Text.Json;
using SWFC.Application.M500_Runtime.M501_Scheduler;
using SWFC.Application.M500_Runtime.M502_Automation;
using SWFC.Application.M500_Runtime.M503_Job_Execution;
using SWFC.Application.M500_Runtime.M504_Control_Leitwarte;
using SWFC.Application.M500_Runtime.M505_Real_Time_Processing;
using SWFC.Architecture.Tests.Support;

namespace SWFC.Architecture.Tests.Rules;

public sealed class M501_M505_V014RuntimeTests
{
    [Fact]
    public void V014_Roadmap_Should_Be_Marked_Done_With_All_Runtime_Milestones_Done()
    {
        using var roadmap = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("docs", "M600-Planning", "M601-Roadmap", "roadmap.json")));

        var version = roadmap.RootElement
            .GetProperty("Versions")
            .EnumerateArray()
            .Single(x => x.GetProperty("Version").GetString() == "v0.14.0");

        Assert.Equal("Done", version.GetProperty("Status").GetString());
        Assert.All(
            new[] { "M501", "M502", "M503", "M504", "M505" },
            module => Assert.Contains(module, ReadStringArray(version, "PrimaryModules")));
        Assert.All(
            version.GetProperty("Milestones").EnumerateArray(),
            milestone => Assert.Equal("Done", milestone.GetProperty("Status").GetString()));
    }

    [Fact]
    public void M501_To_M505_WorkItems_Should_Be_Done_For_V014()
    {
        using var modules = JsonDocument.Parse(File.ReadAllText(
            RepositoryRoot.Combine("src", "SWFC.Web", "wwwroot", "data", "modules.json")));

        var runtimeModules = modules.RootElement
            .GetProperty("Groups")
            .EnumerateArray()
            .SelectMany(group => group.GetProperty("Modules").EnumerateArray())
            .Where(module => (module.GetProperty("Code").GetString() ?? string.Empty) is
                "M501" or "M502" or "M503" or "M504" or "M505")
            .ToArray();

        Assert.Equal(5, runtimeModules.Length);

        foreach (var module in runtimeModules)
        {
            Assert.Equal("Full Complete", module.GetProperty("Status").GetString());
            Assert.Equal(100, module.GetProperty("ProgressPercent").GetInt32());
            Assert.All(
                module.GetProperty("WorkItems").EnumerateArray(),
                item => Assert.Equal("Done", item.GetProperty("Status").GetString()));
        }
    }

    [Fact]
    public void M501_Scheduler_Should_Trigger_Due_Jobs_Once_And_Respect_Control_State()
    {
        var scheduler = new RuntimeScheduler();
        var dueAt = new DateTime(2026, 4, 26, 8, 0, 0, DateTimeKind.Utc);

        scheduler.Register(new SchedulerJobSchedule(
            "maint-due",
            "M202.DueMaintenance",
            SchedulerTriggerKind.DueDate,
            dueAt.AddDays(-1),
            null,
            dueAt,
            "M202",
            new Dictionary<string, string> { ["PlanId"] = "plan-100" }));

        var firstRun = scheduler.Evaluate(dueAt);
        var repeatedRun = scheduler.Evaluate(dueAt.AddMinutes(1));

        Assert.Single(firstRun);
        Assert.Empty(repeatedRun);
        Assert.Equal("Due date reached.", firstRun[0].TriggerReason);
        Assert.Single(scheduler.GetTriggerHistory());

        scheduler.Pause("maint-due");
        Assert.Empty(scheduler.Evaluate(dueAt.AddDays(1)));
        scheduler.Resume("maint-due");
        scheduler.Stop("maint-due");
        Assert.Equal(SchedulerState.Stopped, scheduler.GetSchedules().Single().State);
    }

    [Fact]
    public void M502_Automation_Should_Plan_Actions_And_Block_Unapproved_Critical_Control()
    {
        var engine = new AutomationRuleEngine();
        var timestamp = new DateTime(2026, 4, 26, 9, 0, 0, DateTimeKind.Utc);

        engine.Register(new AutomationRule(
            "inspection-follow-up",
            "Inspection defect creates quality follow-up",
            "InspectionCompleted",
            [new AutomationCondition("Result", "DefectFound")],
            [
                new AutomationAction(
                    AutomationActionKind.EnqueueJob,
                    "M207",
                    "CreateQualityCase",
                    new Dictionary<string, string> { ["Priority"] = "Critical" })
            ]));

        engine.Register(new AutomationRule(
            "unsafe-control",
            "Unsafe direct control is blocked",
            "MachineFaultReported",
            [],
            [
                new AutomationAction(
                    AutomationActionKind.ControlCommand,
                    "M504",
                    "StopMachine",
                    new Dictionary<string, string>(),
                    IsCritical: true)
            ]));

        var planned = engine.Evaluate(new AutomationEvent(
            "InspectionCompleted",
            "M203",
            "inspection-100",
            timestamp,
            new Dictionary<string, string> { ["Result"] = "DefectFound" }));
        var blocked = engine.Evaluate(new AutomationEvent(
            "MachineFaultReported",
            "M404",
            "machine-100",
            timestamp,
            new Dictionary<string, string>()));

        Assert.Contains(planned, decision => decision.Matched && !decision.Blocked && decision.PlannedActions.Count == 1);
        Assert.Contains(blocked, decision => decision.Matched && decision.Blocked);
        Assert.Equal(4, engine.GetDecisionHistory().Count);
    }

    [Fact]
    public async Task M503_Job_Execution_Should_Record_Failures_And_Allow_Defined_Retry()
    {
        var handler = new FlakyJobHandler();
        var executor = new RuntimeJobExecutor([handler]);
        var requestedAt = new DateTime(2026, 4, 26, 10, 0, 0, DateTimeKind.Utc);
        var request = new RuntimeJobRequest(
            "M503.TestJob",
            "M501",
            "trigger-100",
            requestedAt,
            new Dictionary<string, string> { ["Payload"] = "stable" },
            MaxRetries: 1);

        var firstRun = await executor.StartAsync(request, requestedAt);
        var secondRun = await executor.RetryAsync(firstRun.RunId, requestedAt.AddMinutes(1));

        Assert.Equal(RuntimeJobStatus.RetryScheduled, firstRun.Status);
        Assert.Equal(RuntimeJobStatus.Succeeded, secondRun.Status);
        Assert.Equal(2, secondRun.Attempt);
        Assert.Equal(2, executor.GetRuns().Count);
    }

    [Fact]
    public void M504_Control_Should_Expose_Live_State_And_Reject_Uncleared_Interventions()
    {
        var control = new ControlDeskRuntime();
        var timestamp = new DateTime(2026, 4, 26, 11, 0, 0, DateTimeKind.Utc);

        control.PublishSnapshot(new ControlDeskSnapshot(
            "line-a",
            timestamp,
            new Dictionary<string, string> { ["M-100"] = "Running" },
            ["AcknowledgeRequired"]));

        var rejected = control.EvaluateCommand(new ControlCommandRequest(
            "cmd-1",
            "line-a",
            "M-100",
            "StopMachine",
            "user-100",
            timestamp,
            HasPermission: true,
            HasSafetyClearance: false,
            ApprovalReference: "permit-100",
            LocalConfirmationReference: "operator-100"));
        var accepted = control.EvaluateCommand(new ControlCommandRequest(
            "cmd-2",
            "line-a",
            "M-100",
            "StopMachine",
            "user-100",
            timestamp,
            HasPermission: true,
            HasSafetyClearance: true,
            ApprovalReference: "permit-100",
            LocalConfirmationReference: "operator-100"));

        Assert.Single(control.GetSnapshots());
        Assert.Equal(ControlDeskDecisionStatus.Rejected, rejected.Status);
        Assert.Equal(ControlDeskDecisionStatus.Accepted, accepted.Status);
        Assert.Equal(2, control.GetDecisions().Count);
    }

    [Fact]
    public void M505_Real_Time_Processing_Should_Aggregate_Deterministically_And_Stop_Cleanly()
    {
        var processor = new RealTimeProcessor();
        var timestamp = new DateTime(2026, 4, 26, 12, 0, 0, DateTimeKind.Utc);
        var envelope = new RealTimeEnvelope(
            "PCS",
            "line-a",
            timestamp,
            [
                new RealTimePoint("energy.active", "42.0", "kWh", "M205"),
                new RealTimePoint("machine.state", "Running", null, "M212"),
                new RealTimePoint("fault.count", "0", null, "M303")
            ],
            new Dictionary<string, string> { ["Area"] = "A" });

        var first = processor.Process(envelope);
        var second = processor.Process(envelope);
        processor.Stop();
        var stopped = processor.Process(envelope);

        Assert.Equal(first.CurrentValues.OrderBy(x => x.Key), second.CurrentValues.OrderBy(x => x.Key));
        Assert.Equal(1, first.EventCountsByModule["M205"]);
        Assert.Equal("Stopped", stopped.Status);
        Assert.Equal(3, processor.GetHistory().Count);
    }

    [Fact]
    public void M500_Runtime_Modules_Should_Not_Contain_Hidden_Side_Effects_Or_Placeholders()
    {
        var files = RepositoryRoot.EnumerateFiles(
                Path.Combine("src", "SWFC.Application", "M500-Runtime"),
                "*.cs",
                includeGeneratedFiles: false)
            .ToArray();

        Assert.NotEmpty(files);

        var combined = string.Join(Environment.NewLine, files.Select(File.ReadAllText));
        Assert.DoesNotContain("TODO", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Placeholder", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DbContext", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("UseNpgsql", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("NavigationManager", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveChanges", combined, StringComparison.Ordinal);
    }

    private static IReadOnlyCollection<string> ReadStringArray(JsonElement element, string propertyName) =>
        element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .ToArray();

    private sealed class FlakyJobHandler : IRuntimeJobHandler
    {
        private int _calls;

        public string JobKey => "M503.TestJob";

        public Task<RuntimeJobHandlerResult> HandleAsync(
            RuntimeJobRequest request,
            CancellationToken cancellationToken = default)
        {
            _calls++;

            return Task.FromResult(_calls == 1
                ? RuntimeJobHandlerResult.Failure("Controlled failure.")
                : RuntimeJobHandlerResult.Success("Recovered."));
        }
    }
}
