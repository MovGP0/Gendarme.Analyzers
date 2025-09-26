using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(RemoveUnusedLocalVariablesAnalyzer))]
public sealed class RemoveUnusedLocalVariablesAnalyzerTests
{
    [Fact]
    public async Task DetectsUnusedLocal()
    {
        const string source = """
public class Sample
{
    public void M()
    {
        int unused = 42;
    }
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(5, 13, 5, 19).WithArguments("unused"));

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsUsedLocal()
    {
        const string source = """
using System;

public class Sample
{
    public void M()
    {
        int value = 42;
        Console.WriteLine(value);
    }
}
""";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsOutArgument()
    {
        const string source = """
public class Sample
{
    public void M()
    {
        bool.TryParse(string.Empty, out var parsed);
    }
}
""";

        await CreateTest(source).RunAsync();
    }

    private static CSharpAnalyzerTest<RemoveUnusedLocalVariablesAnalyzer, DefaultVerifier> CreateTest(string source) =>
        new()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

    private static DiagnosticResult Diagnostic() =>
        new(DiagnosticId.RemoveUnusedLocalVariables, DiagnosticSeverity.Info);
}