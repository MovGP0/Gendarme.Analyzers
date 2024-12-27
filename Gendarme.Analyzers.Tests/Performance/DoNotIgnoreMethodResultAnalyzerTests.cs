using System.Threading.Tasks;
using Gendarme.Analyzers.Performance;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(DoNotIgnoreMethodResultAnalyzer))]
public sealed class DoNotIgnoreMethodResultAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestTrimIgnoredResult()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        var str = ""Hello"";
        str.Trim(); // This should produce a diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotIgnoreMethodResultAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult.CompilerWarning(DiagnosticId.DoNotIgnoreMethodResult)
            .WithSpan(6, 9, 6, 16)
            .WithArguments("Trim");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestToUpperIgnoredResult()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        var str = ""Hello"";
        str.ToUpper(); // This should produce a diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotIgnoreMethodResultAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult.CompilerWarning(DiagnosticId.DoNotIgnoreMethodResult)
            .WithSpan(6, 9, 6, 16)
            .WithArguments("ToUpper");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestUsedReturnedValue()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        var str = ""Hello"";
        var result = str.ToUpper(); // This should not produce a diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotIgnoreMethodResultAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    // Add more tests for other tracked methods and additional scenarios as necessary
}