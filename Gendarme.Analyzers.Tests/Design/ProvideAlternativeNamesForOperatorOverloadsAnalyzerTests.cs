using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ProvideAlternativeNamesForOperatorOverloadsAnalyzer))]
public sealed class ProvideAlternativeNamesForOperatorOverloadsAnalyzerTests
{
    [Fact]
    public async Task TestOperatorOverloadMissingAlternativeName()
    {
        const string testCode = @"
public class MyClass
{
    public static MyClass operator +(MyClass a) => a;
    public static MyClass operator -(MyClass a) => a;
}";

        var context = new CSharpAnalyzerTest<ProvideAlternativeNamesForOperatorOverloadsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideAlternativeNamesForOperatorOverloads)
            .WithSpan(4, 27, 4, 35) // Assuming the first operator overload location
            .WithArguments("MyClass", "op_UnaryPlus", "Plus");
        
        context.ExpectedDiagnostics.Add(expected);

        expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideAlternativeNamesForOperatorOverloads)
            .WithSpan(5, 27, 5, 35) // Assuming the second operator overload location
            .WithArguments("MyClass", "op_UnaryNegation", "Negate");
        
        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestOperatorOverloadWithAlternativeName()
    {
        const string testCode = @"
public class MyClass
{
    public static MyClass operator +(MyClass a) => a;
    public static MyClass Plus(MyClass a) => a; // Alternative name provided
}";

        var context = new CSharpAnalyzerTest<ProvideAlternativeNamesForOperatorOverloadsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since alternative name is provided.
        await context.RunAsync();
    }
}