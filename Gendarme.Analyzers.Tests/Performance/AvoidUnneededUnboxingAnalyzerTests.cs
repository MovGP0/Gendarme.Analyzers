using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUnneededUnboxingAnalyzer))]
public sealed class AvoidUnneededUnboxingAnalyzerTests
{
    [Fact]
    public async Task TestUnneededUnboxing()
    {
        const string testCode = @"
class MyClass
{
    void Method()
    {
        object obj = new object();
        int number = (int)obj; // Unboxing conversion
        int number2 = (int)obj; // Unboxing conversion
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnneededUnboxingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidUnneededUnboxing)
            .WithSpan(8, 23, 8, 31)
            .WithArguments("int");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}