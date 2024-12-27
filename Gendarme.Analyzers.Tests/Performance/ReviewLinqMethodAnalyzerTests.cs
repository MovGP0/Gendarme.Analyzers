using Gendarme.Analyzers.Performance;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(ReviewLinqMethodAnalyzer))]
public sealed class ReviewLinqMethodAnalyzerTests
{
    [Fact]
    public async Task TestCountMethodInvocation()
    {
        const string testCode = @"
using System;
using System.Linq;
using System.Collections.Generic;

public class MyClass
{
    public void MyMethod(IEnumerable<int> numbers)
    {
        var count = numbers.Count();
    }
}";

        var context = new CSharpAnalyzerTest<ReviewLinqMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.ReviewLinqMethod, DiagnosticSeverity.Info)
            .WithSpan(10, 21, 10, 36)
            .WithArguments("Count");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestLastMethodInvocation()
    {
        const string testCode = @"
using System;
using System.Linq;
using System.Collections.Generic;

public class MyClass
{
    public void MyMethod(IEnumerable<int> numbers)
    {
        var last = numbers.Last();
    }
}";

        var context = new CSharpAnalyzerTest<ReviewLinqMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.ReviewLinqMethod, DiagnosticSeverity.Info)
            .WithSpan(10, 20, 10, 34)
            .WithArguments("Last");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonLinqMethodInvocation_NoDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod()
    {
        var sum = 0;
    }
}";

        var context = new CSharpAnalyzerTest<ReviewLinqMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics
        await context.RunAsync();
    }
}