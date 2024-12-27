using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(AvoidRedundancyInMethodNameAnalyzer))]
public sealed class AvoidRedundancyInMethodNameAnalyzerTests
{
    [Fact]
    public async Task TestRedundantMethodName()
    {
        const string testCode = @"
public class MyClass
{
    public void StringMethod(string input) { }
}
";

        var context = new CSharpAnalyzerTest<AvoidRedundancyInMethodNameAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidRedundancyInMethodName)
            .WithSpan(4, 14, 4, 25)
            .WithArguments("StringMethod", "string");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonRedundantMethodName()
    {
        const string testCode = @"
public class MyClass
{
    public void ProcessData(int data) { }
}
";

        var context = new CSharpAnalyzerTest<AvoidRedundancyInMethodNameAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}