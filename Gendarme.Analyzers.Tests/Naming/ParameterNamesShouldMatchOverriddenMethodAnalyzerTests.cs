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

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ParameterNamesShouldMatchOverriddenMethod)
            .WithSpan(8, 5, 8, 24) // Adjust the position as per the line in your code 
            .WithArguments("MethodA", "a", "x");

        context.ExpectedDiagnostics.Add(expected);

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