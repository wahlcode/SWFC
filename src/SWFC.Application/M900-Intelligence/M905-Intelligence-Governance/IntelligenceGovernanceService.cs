using SWFC.Application.M900_Intelligence;

namespace SWFC.Application.M900_Intelligence.M905_Intelligence_Governance;

public sealed class IntelligenceGovernanceService
{
    public static IntelligenceGovernancePolicy DefaultPolicy { get; } = new(
        RequiresDataBasis: true,
        RequiresExplanation: true,
        AllowsAutomaticActions: false,
        MinimumSourceModuleCount: 1);

    public IntelligenceGovernanceReview Review(
        IntelligenceGovernanceRequest request,
        IntelligenceGovernancePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(request);

        var effectivePolicy = policy ?? DefaultPolicy;
        var violations = new List<string>();

        if (effectivePolicy.RequiresDataBasis && request.SourceModules.Count < effectivePolicy.MinimumSourceModuleCount)
        {
            violations.Add("Missing required source module evidence.");
        }

        if (effectivePolicy.RequiresExplanation && request.Evidence.Count == 0)
        {
            violations.Add("Missing explainability evidence.");
        }

        if (!effectivePolicy.AllowsAutomaticActions && request.ProposesAutomaticAction)
        {
            violations.Add("Automatic actions are not allowed for intelligence results.");
        }

        var status = violations.Count == 0
            ? IntelligenceReviewStatus.ApprovedForReview
            : IntelligenceReviewStatus.Blocked;

        var evidence = new IntelligenceEvidence(
            $"Subject={request.SubjectCode}; Sources={string.Join(",", request.SourceModules.Order(StringComparer.OrdinalIgnoreCase))}; Evidence={request.Evidence.Count}",
            "Governance applies explicit policy checks for data basis, explainability and automatic action control.",
            $"Status={status}; Violations={violations.Count}",
            "M905 never approves automatic execution; it only approves intelligence results for controlled review.");

        return new IntelligenceGovernanceReview(
            request.SubjectCode,
            status,
            violations,
            evidence);
    }
}
