using SWFC.Domain.M100_System.M101_Foundation.Results;

namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Pipeline.Execution;

public interface IExecutionPipeline<TRequest, TResponse>
{
    Task<Result<TResponse>> ExecuteAsync(
        TRequest request,
        CancellationToken cancellationToken = default);
}

