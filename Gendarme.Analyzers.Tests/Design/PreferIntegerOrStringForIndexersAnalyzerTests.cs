using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(PreferIntegerOrStringForIndexersAnalyzer))]
public sealed class PreferIntegerOrStringForIndexersAnalyzerTests
{
    [Fact]
    public async Task TestIndexerWithNonIntegerOrStringParameter()
    {
        const string testCode = @"
public class MyClass
{
    public int this[float index] { get { return 0; } }
}
";

        var context = new CSharpAnalyzerTest<PreferIntegerOrStringForIndexersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferIntegerOrStringForIndexers)
            .WithSpan(4, 21, 4, 26)
            .WithArguments("MyClass", "System.Single");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestIndexerWithValidIntegerParameter()
    {
        const string testCode = @"
public class MyClass
{
    public int this[int index] { get { return 0; } }
}
";

        var context = new CSharpAnalyzerTest<PreferIntegerOrStringForIndexersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestIndexerWithValidStringParameter()
    {
        const string testCode = @"
public class MyClass
{
    public string this[string key] { get { return ""value""; } }
}
";

        var context = new CSharpAnalyzerTest<PreferIntegerOrStringForIndexersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}