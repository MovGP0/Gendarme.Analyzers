using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(UsePreferredTermsAnalyzer))]
public sealed class UsePreferredTermsAnalyzerTests
{
    [Fact]
    public async Task TestUsePreferredTermsAnalyzer_CancellingKeyword()
    {
        const string testCode = @"
public class TestClass
{
    public void CancelledMethod() { }
}";

        var context = new CSharpAnalyzerTest<UsePreferredTermsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UsePreferredTerms)
            .WithSpan(4, 19, 4, 26)
            .WithArguments("Cancelled", "Canceled");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }

    [Fact]
    public async Task TestUsePreferredTermsAnalyzer_NegativeKeywords()
    {
        const string testCode = @"
public class TestClass
{
    public void CancelMethod() { }
}";

        var context = new CSharpAnalyzerTest<UsePreferredTermsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostic expected since "Cancel" is not a preferred term
        await context.RunAsync();
    }

    [Fact]
    public async Task TestUsePreferredTermsAnalyzer_WithMultipleTerms()
    {
        const string testCode = @"
public class TestClass
{
    public void WereNotMethod() { }
}";

        var context = new CSharpAnalyzerTest<UsePreferredTermsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UsePreferredTerms)
            .WithSpan(4, 19, 4, 27)
            .WithArguments("WereNot", "WereNot");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }
}