namespace SWFC.Infrastructure.Services.System;

public interface IM102DataInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}