using SWFC.Domain.Common.Results;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;

public interface IPipelineStep<TRequest, TResponse>
{
    Task<Result> ExecuteAsync(
        IPipelineContext<TRequest> context,
        CancellationToken cancellationToken = default);
}
