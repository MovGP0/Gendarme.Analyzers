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

        var expected = new DiagnosticResult(DiagnosticId.UsePreferredTerms, DiagnosticSeverity.Info)
            .WithSpan(4, 17, 4, 32)
            .WithArguments("CancelledMethod", "Cancelled", "Canceled");

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
    public void WerentMethod() { }
}";

        var context = new CSharpAnalyzerTest<UsePreferredTermsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.UsePreferredTerms, DiagnosticSeverity.Info)
            .WithSpan(4, 17, 4, 29)
            .WithArguments("WerentMethod", "Werent", "WereNot");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }
}