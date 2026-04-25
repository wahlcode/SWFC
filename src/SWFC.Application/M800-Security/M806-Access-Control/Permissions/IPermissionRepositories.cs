namespace SWFC.Application.M800_Security.M806_AccessControl.Permissions;

public interface IPermissionReadRepository
{
    Task<IReadOnlyList<PermissionListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
