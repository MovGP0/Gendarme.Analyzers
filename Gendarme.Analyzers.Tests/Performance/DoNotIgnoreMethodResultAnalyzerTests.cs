using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(DoNotIgnoreMethodResultAnalyzer))]
public sealed class DoNotIgnoreMethodResultAnalyzerTests
{
    [Fact]
    public async Task DetectsIgnoredStringMethod()
    {
        const string source = """
using System;

public class Sample
{
    public void M()
    {
        var text = "Hello";
        text.Trim();
    }
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(8, 9, 8, 20).WithArguments("Trim"));

        await test.RunAsync();
    }

    [Fact]
    public async Task DetectsIgnoredEnumerableMethod()
    {
        const string source = """
using System.Collections.Generic;
using System.Linq;

public static class Sample
{
    public static void M()
    {
        var values = new List<int>();
        Enumerable.Reverse(values);
    }
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(9, 9, 9, 35).WithArguments("Reverse"));

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsWhenResultIsAssigned()
    {
        const string source = """
using System;

public class Sample
{
    public void M()
    {
        var text = "Hello";
        var trimmed = text.Trim();
        Console.WriteLine(trimmed);
    }
}
""";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsVoidReturningMethod()
    {
        const string source = """
using System;

public class Sample
{
    public void M()
    {
        Console.WriteLine("value");
    }
}
""";

        await CreateTest(source).RunAsync();
    }

    private static CSharpAnalyzerTest<DoNotIgnoreMethodResultAnalyzer, DefaultVerifier> CreateTest(string source) =>
        new()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

    private static DiagnosticResult Diagnostic() =>
        new(DiagnosticId.DoNotIgnoreMethodResult, DiagnosticSeverity.Warning);
}