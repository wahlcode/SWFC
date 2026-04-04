namespace SWFC.Application.M100_System.M102_Organization.Interfaces;

public interface IRolePermissionMapper
{
    IReadOnlyCollection<string> Map(IEnumerable<string> roles);
}