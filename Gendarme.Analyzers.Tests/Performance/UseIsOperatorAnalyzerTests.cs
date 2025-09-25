using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(UseIsOperatorAnalyzer))]
public sealed class UseIsOperatorAnalyzerTests
{
    [Fact]
    public async Task TestUseIsOperator()
    {
        const string testCode = @"
class MyClass
{
    void MyMethod(object obj)
    {
        if (obj as string != null) // This should trigger a warning
        {
            // Do something
        }
    }
}";

        var context = new CSharpAnalyzerTest<UseIsOperatorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseIsOperator)
            .WithSpan(6, 9, 6, 26)
            .WithArguments("(obj as string) != null");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoUseIsOperatorWarning()
    {
        const string testCode = @"
class MyClass
{
    void MyMethod(object obj)
    {
        if (obj != null) // This should not trigger a warning
        {
            // Do something
        }
    }
}";

        var context = new CSharpAnalyzerTest<UseIsOperatorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}