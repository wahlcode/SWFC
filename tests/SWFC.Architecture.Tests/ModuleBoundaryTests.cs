using System.Reflection;

namespace SWFC.Architecture.Tests;

public sealed class ModuleBoundaryTests
{
    [Fact]
    public void Application_Modules_Should_Not_Depend_On_Other_Application_Modules()
    {
        var assembly = typeof(SWFC.Application.AssemblyMarker).Assembly;

        var violations = FindCrossModuleDependencies(
            assembly,
            rootNamespace: "SWFC.Application.Modules",
            commonNamespacePrefix: "SWFC.Application.Common");

        Assert.True(
            violations.Count == 0,
            "Application module boundary violations:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void Domain_Modules_Should_Not_Depend_On_Other_Domain_Modules()
    {
        var assembly = typeof(SWFC.Domain.AssemblyMarker).Assembly;

        var violations = FindCrossModuleDependencies(
            assembly,
            rootNamespace: "SWFC.Domain.Modules",
            commonNamespacePrefix: "SWFC.Domain.Common");

        Assert.True(
            violations.Count == 0,
            "Domain module boundary violations:" + Environment.NewLine + string.Join(Environment.NewLine, violations));
    }

    private static List<string> FindCrossModuleDependencies(
        Assembly assembly,
        string rootNamespace,
        string commonNamespacePrefix)
    {
        var violations = new List<string>();

        var moduleTypes = assembly.GetTypes()
            .Where(t => t.Namespace is not null && t.Namespace.StartsWith(rootNamespace + ".", StringComparison.Ordinal))
            .ToArray();

        foreach (var type in moduleTypes)
        {
            var sourceModule = ExtractModuleName(type.Namespace!, rootNamespace);
            if (sourceModule is null)
            {
                continue;
            }

            var referencedTypes = type
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .SelectMany(GetReferencedTypes)
                .Concat(GetBaseAndInterfaceTypes(type))
                .Where(t => t.Namespace is not null)
                .Distinct()
                .ToArray();

            foreach (var referencedType in referencedTypes)
            {
                var ns = referencedType.Namespace!;
                if (ns.StartsWith(commonNamespacePrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!ns.StartsWith(rootNamespace + ".", StringComparison.Ordinal))
                {
                    continue;
                }

                var targetModule = ExtractModuleName(ns, rootNamespace);
                if (targetModule is null)
                {
                    continue;
                }

                if (!string.Equals(sourceModule, targetModule, StringComparison.Ordinal))
                {
                    violations.Add($"{type.FullName} -> {referencedType.FullName}");
                }
            }
        }

        return violations
            .Distinct()
            .OrderBy(v => v)
            .ToList();
    }

    private static string? ExtractModuleName(string ns, string rootNamespace)
    {
        var prefix = rootNamespace + ".";
        if (!ns.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        var rest = ns[prefix.Length..];
        var firstDot = rest.IndexOf('.');
        return firstDot < 0 ? rest : rest[..firstDot];
    }

    private static IEnumerable<Type> GetBaseAndInterfaceTypes(Type type)
    {
        if (type.BaseType is not null)
        {
            yield return type.BaseType;
        }

        foreach (var i in type.GetInterfaces())
        {
            yield return i;
        }
    }

    private static IEnumerable<Type> GetReferencedTypes(MemberInfo member)
    {
        return member switch
        {
            FieldInfo f => UnwrapTypes(f.FieldType),
            PropertyInfo p => UnwrapTypes(p.PropertyType),
            MethodInfo m => UnwrapTypes(m.ReturnType)
                .Concat(m.GetParameters().SelectMany(p => UnwrapTypes(p.ParameterType))),
            ConstructorInfo c => c.GetParameters().SelectMany(p => UnwrapTypes(p.ParameterType)),
            EventInfo e => UnwrapTypes(e.EventHandlerType),
            _ => Enumerable.Empty<Type>()
        };
    }

    private static IEnumerable<Type> UnwrapTypes(Type? type)
    {
        if (type is null)
        {
            yield break;
        }

        if (type.IsGenericType)
        {
            yield return type;

            foreach (var arg in type.GetGenericArguments())
            {
                foreach (var nested in UnwrapTypes(arg))
                {
                    yield return nested;
                }
            }

            yield break;
        }

        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (var nested in UnwrapTypes(elementType))
            {
                yield return nested;
            }

            yield break;
        }

        yield return type;
    }
}