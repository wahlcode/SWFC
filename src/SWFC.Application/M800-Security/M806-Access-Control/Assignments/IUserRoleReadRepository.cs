namespace SWFC.Application.M800_Security.M806_AccessControl.Assignments;

public interface IUserRoleReadRepository
{
    Task<IReadOnlyList<Guid>> GetActiveRoleIdsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetActiveRoleNamesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
