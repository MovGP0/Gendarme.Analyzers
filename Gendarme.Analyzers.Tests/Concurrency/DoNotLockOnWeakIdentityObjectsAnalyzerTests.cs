using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotLockOnWeakIdentityObjectsAnalyzer))]
public sealed class DoNotLockOnWeakIdentityObjectsAnalyzerTests
{
    [Fact]
    public async Task TestLockOnWeakIdentityObject()
    {
        const string testCode = @"
using System.Threading;

public class MyClass
{
    public void MyMethod()
    {
        lock (string.Empty) // Locking on a weak identity object
        {
            // Critical section
        }
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotLockOnWeakIdentityObjectsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotLockOnWeakIdentityObjects)
            .WithSpan(6, 14, 6, 28) // Adjusted location based on actual code
            .WithArguments("System.String");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestLockOnNonWeakIdentityObject()
    {
        const string testCode = @"
public class MyClass
{
    private readonly object _lock = new object();

    public void MyMethod()
    {
        lock (_lock) // Locking on a strong identity object
        {
            // Critical section
        }
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotLockOnWeakIdentityObjectsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}