using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;

public interface IUseCaseHandler<TRequest, TResponse>
{
    Task<Result<TResponse>> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
}

