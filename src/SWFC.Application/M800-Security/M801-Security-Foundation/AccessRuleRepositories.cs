using SWFC.Domain.M100_System.M101_Foundation.ValueObjects;
using SWFC.Domain.M800_Security.M801_Access;

namespace SWFC.Application.M800_Security.M801_Access;

public sealed record AccessRuleTargetRef(
    AccessTargetType TargetType,
    string TargetId);

public interface IAccessRuleReadRepository
{
    Task<IReadOnlyList<AccessRule>> GetByTargetsAsync(
        IReadOnlyCollection<AccessRuleTargetRef> targets,
        CancellationToken cancellationToken = default);
}

public interface IAccessRuleWriteRepository
{
    Task AddAsync(AccessRule accessRule, CancellationToken cancellationToken = default);
    Task<AccessRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    void Deactivate(AccessRule accessRule, ChangeContext changeContext);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
