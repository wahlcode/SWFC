namespace SWFC.Infrastructure.M100_System.M107_SetupDeployment;

public interface IM107SetupInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
