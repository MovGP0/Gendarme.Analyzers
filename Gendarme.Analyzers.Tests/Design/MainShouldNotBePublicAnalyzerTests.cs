using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(MainShouldNotBePublicAnalyzer))]
public sealed class MainShouldNotBePublicAnalyzerTests
{
    [Fact]
    public async Task TestPublicMainMethod()
    {
        const string testCode = @"
using System;

public class Program
{
    public static void Main()
    {
    }
}";

        var context = new CSharpAnalyzerTest<MainShouldNotBePublicAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MainShouldNotBePublic)
            .WithSpan(6, 24, 6, 28)
            .WithArguments("Main");
        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonPublicMainMethod()
    {
        const string testCode = @"
using System;

public class Program
{
    static void Main()
    {
    }
}";

        var context = new CSharpAnalyzerTest<MainShouldNotBePublicAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}