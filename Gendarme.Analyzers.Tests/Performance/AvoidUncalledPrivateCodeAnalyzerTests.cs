using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUncalledPrivateCodeAnalyzer))]
public sealed class AvoidUncalledPrivateCodeAnalyzerTests
{
    [Fact]
    public async Task TestUncalledPrivateMethod()
    {
        const string testCode = @"
public class MyClass
{
    private void UncalledMethod() { }

    public void CalledMethod()
    {
        UncalledMethod();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidUncalledPrivateCodeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidUncalledPrivateCode)
            .WithSpan(3, 14, 3, 30)
            .WithArguments("UncalledMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPrivateMethodIsUncalled()
    {
        const string testCode = @"
public class MyClass
{
    private void UncalledMethod() { }
    internal void InternalMethod() { }
}
";

        var context = new CSharpAnalyzerTest<AvoidUncalledPrivateCodeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidUncalledPrivateCode)
            .WithSpan(4, 18, 4, 32)
            .WithArguments("UncalledMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPrivateMethodCalled()
    {
        const string testCode = @"
public class MyClass
{
    private void UncalledMethod() { }

    public void CalledMethod()
    {
        UncalledMethod();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidUncalledPrivateCodeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since UncalledMethod is called.
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoPrivateMethod()
    {
        const string testCode = @"
public class MyClass
{
    public void PublicMethod() { }
}
";

        var context = new CSharpAnalyzerTest<AvoidUncalledPrivateCodeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since there are no private methods.
        await context.RunAsync();
    }
}