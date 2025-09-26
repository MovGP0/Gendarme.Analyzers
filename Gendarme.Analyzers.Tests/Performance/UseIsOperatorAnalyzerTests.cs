using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(UseIsOperatorAnalyzer))]
public sealed class UseIsOperatorAnalyzerTests
{
    [Fact]
    public async Task DetectsAsNotEqualsNull()
    {
        const string source = """
class Sample
{
    void M(object value)
    {
        if (value as string != null)
        {
        }
    }
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(6, 9, 6, 34).WithArguments("value as string != null"));

        await test.RunAsync();
    }

    [Fact]
    public async Task DetectsAsEqualsNull()
    {
        const string source = """
class Sample
{
    void M(object value)
    {
        if ((value as string) == null)
        {
        }
    }
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(6, 9, 6, 38).WithArguments("(value as string) == null"));

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsIsCheck()
    {
        const string source = """
class Sample
{
    void M(object value)
    {
        if (value is string)
        {
        }
    }
}
""";

        await CreateTest(source).RunAsync();
    }

    private static CSharpAnalyzerTest<UseIsOperatorAnalyzer, DefaultVerifier> CreateTest(string source) =>
        new()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

    private static DiagnosticResult Diagnostic() =>
        new(DiagnosticId.UseIsOperator, DiagnosticSeverity.Info);
}