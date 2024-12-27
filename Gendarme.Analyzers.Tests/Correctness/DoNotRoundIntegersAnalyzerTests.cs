using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(DoNotRoundIntegersAnalyzer))]
public sealed class DoNotRoundIntegersAnalyzerTests
{
    [Fact]
    public async Task TestRoundingFloatingPointArgument()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        Math.Round(3.5);
    }
}";

        var context = new CSharpAnalyzerTest<DoNotRoundIntegersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoDiagnosticForNonIntegralArgument()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        Math.Round(3.14);
    }
}";

        var context = new CSharpAnalyzerTest<DoNotRoundIntegersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}