using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(ParameterNamesShouldMatchOverriddenMethodAnalyzer))]
public sealed class ParameterNamesShouldMatchOverriddenMethodAnalyzerTests
{
    [Fact]
    public async Task TestParameterNamesMismatch()
    {
        const string testCode = @"
public class BaseClass
{
    public virtual void MethodA(int x, string y) { }
}

public class DerivedClass : BaseClass
{
    public override void MethodA(int a, string b) { }
}";

        var context = new CSharpAnalyzerTest<ParameterNamesShouldMatchOverriddenMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = DiagnosticResult
            .CompilerWarning(DiagnosticId.ParameterNamesShouldMatchOverriddenMethod)
            .WithSpan(9, 38, 9, 39)
            .WithArguments("MethodA", "a", "x");

        var expected2 = DiagnosticResult
            .CompilerWarning(DiagnosticId.ParameterNamesShouldMatchOverriddenMethod)
            .WithSpan(9, 48, 9, 49)
            .WithArguments("MethodA", "b", "y");

        context.ExpectedDiagnostics.Add(expected1);
        context.ExpectedDiagnostics.Add(expected2);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestParameterNamesMatch()
    {
        const string testCode = @"
public class BaseClass
{
    public virtual void MethodA(int x, string y) { }
}

public class DerivedClass : BaseClass
{
    public override void MethodA(int x, string y) { }
}";

        var context = new CSharpAnalyzerTest<ParameterNamesShouldMatchOverriddenMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since parameter names match
        await context.RunAsync();
    }
}