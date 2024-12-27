using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotUseLockedRegionOutsideMethodAnalyzer))]
public sealed class DoNotUseLockedRegionOutsideMethodAnalyzerTests
{
    [Fact]
    public async Task TestLockedUsageOutsideMethod()
    {
        const string testCode = @"
using System.Threading;

public class MyClass
{
    public void Foo()
    {
        Monitor.Enter(this);
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotUseLockedRegionOutsideMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotUseLockedRegionOutsideMethod)
            .WithSpan(8, 9, 8, 28);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestLockedUsageInsideMethod()
    {
        const string testCode = @"
using System.Threading;

public class MyClass
{
    public void Foo()
    {
        Monitor.Enter(this);
        Monitor.Exit(this);
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotUseLockedRegionOutsideMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}