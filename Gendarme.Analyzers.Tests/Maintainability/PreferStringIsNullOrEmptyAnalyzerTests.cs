using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(PreferStringIsNullOrEmptyAnalyzer))]
public sealed class PreferStringIsNullOrEmptyAnalyzerTests
{
    [Fact]
    public async Task TestStringIsNullOrEmptyCheck()
    {
        const string testCode = @"
class TestClass
{
    void TestMethod(string str)
    {
        if (str == null || str.Length == 0) { }
        if (str != null && str.Length > 0) { }
    }
}";

        var context = new CSharpAnalyzerTest<PreferStringIsNullOrEmptyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expectedNullCheck = new DiagnosticResult(DiagnosticId.PreferStringIsNullOrEmpty, DiagnosticSeverity.Info)
            .WithSpan(6, 13, 6, 43);

        var expectedNotNullCheck = new DiagnosticResult(DiagnosticId.PreferStringIsNullOrEmpty, DiagnosticSeverity.Info)
            .WithSpan(7, 13, 7, 42);

        context.ExpectedDiagnostics.Add(expectedNullCheck);
        context.ExpectedDiagnostics.Add(expectedNotNullCheck);

        await context.RunAsync();
    }
}