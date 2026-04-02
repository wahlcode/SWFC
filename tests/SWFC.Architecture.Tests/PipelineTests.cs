using NetArchTest.Rules;
using Xunit;

namespace SWFC.Architecture.Tests;

public class PipelineTests
{
    [Fact]
    public void Handlers_Must_Implement_IUseCaseHandler()
    {
        var result = Types.InAssembly(typeof(SWFC.Application.AssemblyMarker).Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ImplementInterface(typeof(SWFC.Application.M800_Security.M802_ApplicationSecurity.Abstractions.IUseCaseHandler<,>))
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void No_Handler_Should_Be_Called_Directly()
    {
        var result = Types.InAssembly(typeof(SWFC.Web.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Handler")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}