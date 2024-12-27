using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidLongParameterListsAnalyzer))]
public sealed class AvoidLongParameterListsAnalyzerTests
{
    [Fact]
    public async Task TestMethodWithTooManyParameters_ReportsDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod(int a, int b, int c, int d, int e, int f, int g) { }
}";

        var context = new CSharpAnalyzerTest<AvoidLongParameterListsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult.CompilerWarning(DiagnosticId.AvoidLongParameterLists)
            .WithSpan(4, 17, 4, 25)
            .WithArguments("MyMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodWithMaxParameters_NoDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod(int a, int b, int c, int d, int e, int f) { }
}";

        var context = new CSharpAnalyzerTest<AvoidLongParameterListsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since the method has exactly 6 parameters
        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodWithFewerParameters_NoDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod(int a, int b, int c) { }
}";

        var context = new CSharpAnalyzerTest<AvoidLongParameterListsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since the method has fewer than 6 parameters
        await context.RunAsync();
    }
}