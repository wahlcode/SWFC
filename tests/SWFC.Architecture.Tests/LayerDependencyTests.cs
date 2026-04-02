using NetArchTest.Rules;

namespace SWFC.Architecture.Tests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types.InAssembly(typeof(SWFC.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SWFC.Application",
                "SWFC.Infrastructure",
                "SWFC.Web")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure_Or_Web()
    {
        var result = Types.InAssembly(typeof(SWFC.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "SWFC.Infrastructure",
                "SWFC.Web")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Web()
    {
        var result = Types.InAssembly(typeof(SWFC.Infrastructure.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("SWFC.Web")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_EntityFrameworkCore()
    {
        var result = Types.InAssembly(typeof(SWFC.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_AspNetCore()
    {
        var result = Types.InAssembly(typeof(SWFC.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_AspNetCore()
    {
        var result = Types.InAssembly(typeof(SWFC.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result.FailingTypeNames));
    }

    [Fact]
    public void Application_Should_Not_Depend_On_EntityFrameworkCore()
    {
        var result = Types.InAssembly(typeof(SWFC.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, BuildMessage(result.FailingTypeNames));
    }

    private static string BuildMessage(IEnumerable<string> failingTypes)
    {
        var items = failingTypes?.ToArray() ?? [];
        return items.Length == 0
            ? "Architecture rule failed."
            : $"Architecture violation(s): {string.Join(", ", items)}";
    }

    [Fact]
    public void Application_Should_Not_Use_DbContext()
    {
        var result = Types.InAssembly(typeof(SWFC.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("AppDbContext")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}