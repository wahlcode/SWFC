namespace SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions;

public interface IPipelineContext<out TRequest>
{
    TRequest Request { get; }
    SecurityContext SecurityContext { get; }
}

