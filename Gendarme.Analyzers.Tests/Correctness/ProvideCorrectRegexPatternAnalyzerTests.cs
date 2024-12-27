using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ProvideCorrectRegexPatternAnalyzer))]
public sealed class ProvideCorrectRegexPatternAnalyzerTests
{
    [Fact]
    public async Task TestInvalidRegexPattern()
    {
        const string testCode = @"
using System.Text.RegularExpressions;

public class MyClass
{
    public void MyMethod()
    {
        var regex = new Regex(""***InvalidPattern***"");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideCorrectRegexPatternAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideCorrectRegexPattern)
            .WithSpan(8, 31, 8, 53)
            .WithArguments("Regex");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidRegexPattern()
    {
        const string testCode = @"
using System.Text.RegularExpressions;

public class MyClass
{
    public void MyMethod()
    {
        var regex = new Regex(""[a-zA-Z]+"");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideCorrectRegexPatternAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}