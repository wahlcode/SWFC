namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;

public sealed class PipelineContext<TRequest> : IPipelineContext<TRequest>
{
    public PipelineContext(TRequest request, SecurityContext securityContext)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        SecurityContext = securityContext ?? throw new ArgumentNullException(nameof(securityContext));
    }

    public TRequest Request { get; }

    public SecurityContext SecurityContext { get; }
}

