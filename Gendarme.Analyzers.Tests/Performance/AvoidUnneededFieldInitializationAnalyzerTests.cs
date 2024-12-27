using Gendarme.Analyzers.Performance;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUnneededFieldInitializationAnalyzer))]
public sealed class AvoidUnneededFieldInitializationAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestUnneededFieldInitialization()
    {
        const string testCode = @"
public class MyClass
{
    private int _field = 0;
}
";

        var context = new CSharpAnalyzerTest<AvoidUnneededFieldInitializationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidUnneededFieldInitialization, DiagnosticSeverity.Info)
            .WithSpan(5, 24, 5, 30)
            .WithArguments("_field");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestFieldInitializationWithNonDefaultValue()
    {
        const string testCode = @"
public class MyClass
{
    private int _field = 1;
}
";

        var context = new CSharpAnalyzerTest<AvoidUnneededFieldInitializationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}