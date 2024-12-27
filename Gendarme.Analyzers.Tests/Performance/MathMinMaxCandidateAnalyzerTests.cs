using Gendarme.Analyzers.Performance;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(MathMinMaxCandidateAnalyzer))]
public sealed class MathMinMaxCandidateAnalyzerTests
{
    [Fact]
    public async Task TestMinMaxPatternWithGreaterThan()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public int CompareValues(int a, int b)
    {
        return (a > b) ? a : b;
    }
}
";

        var context = new CSharpAnalyzerTest<MathMinMaxCandidateAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.MathMinMaxCandidate, DiagnosticSeverity.Info)
            .WithSpan(8, 16, 8, 31)
            .WithArguments("Max");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMinMaxPatternWithLessThan()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public int CompareValues(int a, int b)
    {
        return (a < b) ? a : b;
    }
}
";

        var context = new CSharpAnalyzerTest<MathMinMaxCandidateAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.MathMinMaxCandidate, DiagnosticSeverity.Info)
            .WithSpan(8, 16, 8, 31)
            .WithArguments("Min");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoMinMaxPattern()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public int CompareValues(int a, int b)
    {
        return a + b; // This should not trigger a diagnostic
    }
}
";

        var context = new CSharpAnalyzerTest<MathMinMaxCandidateAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics
        await context.RunAsync();
    }
}