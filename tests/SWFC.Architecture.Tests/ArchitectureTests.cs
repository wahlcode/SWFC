using NetArchTest.Rules;
using Xunit;

namespace SWFC.Architecture.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types.InAssembly(typeof(SWFC.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SWFC.Application")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(SWFC.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SWFC.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Web()
    {
        var result = Types.InAssembly(typeof(SWFC.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SWFC.Web")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Web()
    {
        var result = Types.InAssembly(typeof(SWFC.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SWFC.Web")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Web()
    {
        var result = Types.InAssembly(typeof(SWFC.Infrastructure.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SWFC.Web")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Web_Should_Not_Depend_On_Infrastructure_Persistence()
    {
        var result = Types.InAssembly(typeof(SWFC.Web.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("SWFC.Infrastructure.Persistence")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}