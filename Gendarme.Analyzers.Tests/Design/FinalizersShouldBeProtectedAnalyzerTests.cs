using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(FinalizersShouldBeProtectedAnalyzer))]
public sealed class FinalizersShouldBeProtectedAnalyzerTests
{
    [Fact]
    public async Task TestFinalizerNotProtected()
    {
        const string testCode = @"
public class MyClass
{
    ~MyClass() { }
}
";

        var context = new CSharpAnalyzerTest<FinalizersShouldBeProtectedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.FinalizersShouldBeProtected)
            .WithSpan(4, 5, 4, 13)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestFinalizerProtected()
    {
        const string testCode = @"
public class MyClass
{
    protected ~MyClass() { }
}
";

        var context = new CSharpAnalyzerTest<FinalizersShouldBeProtectedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}