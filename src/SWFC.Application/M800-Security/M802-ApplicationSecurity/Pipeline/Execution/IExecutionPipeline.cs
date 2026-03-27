using SWFC.Domain.Common.Results;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;

public interface IExecutionPipeline<TRequest, TResponse>
{
    Task<Result<TResponse>> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
}
