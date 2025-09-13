using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(PreferEventsOverMethodsAnalyzer))]
public sealed class PreferEventsOverMethodsAnalyzerTests
{
    [Fact]
    public async Task TestMethodNamedRaiseTriggersDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void RaiseEvent() { }
}
";

        var context = new CSharpAnalyzerTest<PreferEventsOverMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.PreferEventsOverMethods, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithSpan(4, 17, 4, 27)
            .WithArguments("RaiseEvent");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodNamedOnTriggersDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void OnSomething() { }
}
";

        var context = new CSharpAnalyzerTest<PreferEventsOverMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.PreferEventsOverMethods, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithSpan(4, 17, 4, 28)
            .WithArguments("OnSomething");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodNamedFireTriggersDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void FireEvent() { }
}
";

        var context = new CSharpAnalyzerTest<PreferEventsOverMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.PreferEventsOverMethods, Microsoft.CodeAnalysis.DiagnosticSeverity.Info)
            .WithSpan(4, 17, 4, 26)
            .WithArguments("FireEvent");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestRegularMethodDoesNotTriggerDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void DoSomething() { }
}
";

        var context = new CSharpAnalyzerTest<PreferEventsOverMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostic

        await context.RunAsync();
    }
}