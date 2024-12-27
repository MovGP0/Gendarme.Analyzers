using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewInconsistentIdentityAnalyzer))]
public sealed class ReviewInconsistentIdentityAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestInconsistentIdentity()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public override bool Equals(object obj)
    {
        return obj is MyClass;
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public int CompareTo(MyClass other)
    {
        return 0;
    }
}
";

        var context = new CSharpAnalyzerTest<ReviewInconsistentIdentityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewInconsistentIdentity)
            .WithSpan(7, 14, 7, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestConsistentIdentity()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public override bool Equals(object obj)
    {
        if (obj is not MyClass)
            return false;
        return true;
    }

    public override int GetHashCode()
    {
        return 1;
    }

    public int CompareTo(MyClass other)
    {
        return 1;
    }
}
";

        var context = new CSharpAnalyzerTest<ReviewInconsistentIdentityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}