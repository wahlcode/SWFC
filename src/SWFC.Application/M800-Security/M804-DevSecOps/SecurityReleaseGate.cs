using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M804_DevSecOps;

public sealed record SecurityReleaseEvidence(
    string BuildId,
    bool BuildSigned,
    bool SourceReviewed,
    bool DependencyScanPassed,
    bool SecurityTestsPassed,
    bool DeploymentApproved,
    IReadOnlyCollection<string>? CriticalFindings = null);

public interface ISecurityReleaseGate
{
    Result Verify(SecurityReleaseEvidence evidence);
}

public sealed class SecurityReleaseGate : ISecurityReleaseGate
{
    public Result Verify(SecurityReleaseEvidence evidence)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(evidence.BuildId))
        {
            missing.Add("build id");
        }

        if (!evidence.BuildSigned)
        {
            missing.Add("signed build");
        }

        if (!evidence.SourceReviewed)
        {
            missing.Add("source review");
        }

        if (!evidence.DependencyScanPassed)
        {
            missing.Add("dependency scan");
        }

        if (!evidence.SecurityTestsPassed)
        {
            missing.Add("security tests");
        }

        if (!evidence.DeploymentApproved)
        {
            missing.Add("deployment approval");
        }

        if (evidence.CriticalFindings?.Count > 0)
        {
            missing.Add("no critical security findings");
        }

        return missing.Count == 0
            ? Result.Success()
            : Result.Failure(new Error(
                "m804.release_gate.blocked",
                $"Release blocked: {string.Join(", ", missing)}.",
                ErrorCategory.Security));
    }
}
