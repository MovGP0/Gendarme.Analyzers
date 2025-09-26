using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidRepetitiveCastsAnalyzer))]
public sealed class AvoidRepetitiveCastsAnalyzerTests
{
    [Fact]
    public async Task TestRepetitiveCasts()
    {
        const string testCode = @"
class MyClass
{
    void MyMethod(object obj)
    {
        var cast1 = (string)obj;
        var cast2 = (string)obj;
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidRepetitiveCastsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidRepetitiveCasts)
            .WithSpan(7, 21, 7, 32)
            .WithArguments("obj", "string");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}