using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(CompareWithEmptyStringEfficientlyAnalyzer))]
public sealed class CompareWithEmptyStringEfficientlyAnalyzerTests
{
    [Fact]
    public async Task TestCompareWithLiteralEmptyString()
    {
        const string testCode = @"
public class MyClass
{
    public void Method(string input)
    {
        if (input == """") { }
    }
}";

        var context = new CSharpAnalyzerTest<CompareWithEmptyStringEfficientlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.CompareWithEmptyStringEfficiently, DiagnosticSeverity.Info)
            .WithSpan(6, 13, 6, 24)
            .WithArguments("input == \"\"");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }

    [Fact]
    public async Task TestCompareWithStringEmpty()
    {
        const string testCode = @"
public class MyClass
{
    public void Method(string input)
    {
        if (input == string.Empty) { }
    }
}";

        var context = new CSharpAnalyzerTest<CompareWithEmptyStringEfficientlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.CompareWithEmptyStringEfficiently, DiagnosticSeverity.Info)
            .WithSpan(6, 13, 6, 34)
            .WithArguments("input == string.Empty");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }

    [Fact]
    public async Task TestCompareWithStringEmptyAlias()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void Method(string input)
    {
        if (input == String.Empty) { }
    }
}";

        var context = new CSharpAnalyzerTest<CompareWithEmptyStringEfficientlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.CompareWithEmptyStringEfficiently, DiagnosticSeverity.Info)
            .WithSpan(8, 13, 8, 34)
            .WithArguments("input == String.Empty");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoComparisonWithNonEmptyStrings()
    {
        const string testCode = @"
public class MyClass
{
    public void Method(string input)
    {
        if (input == ""hello"") { }
    }
}";

        var context = new CSharpAnalyzerTest<CompareWithEmptyStringEfficientlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics
        await context.RunAsync();
    }
}