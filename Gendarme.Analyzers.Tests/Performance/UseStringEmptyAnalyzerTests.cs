using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(UseStringEmptyAnalyzer))]
public sealed class UseStringEmptyAnalyzerTests
{
    [Fact]
    public async Task TestStringLiteralEmpty()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod()
            {
                string myString = """";
            }
        }";

        var context = new CSharpAnalyzerTest<UseStringEmptyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.UseStringEmpty, DiagnosticSeverity.Info)
            .WithSpan(6, 35, 6, 37);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}