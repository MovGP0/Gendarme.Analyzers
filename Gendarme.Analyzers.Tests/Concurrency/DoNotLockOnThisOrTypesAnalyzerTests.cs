using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotLockOnThisOrTypesAnalyzer))]
public sealed class DoNotLockOnThisOrTypesAnalyzerTests
{
    [Fact]
    public async Task TestLockOnThis()
    {
        const string testCode = @"
class MyClass
{
    public void MyMethod()
    {
        lock (this) { }
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotLockOnThisOrTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotLockOnThisOrTypes)
            .WithSpan(6, 15, 6, 19);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestLockOnTypeOf()
    {
        const string testCode = @"
class MyClass
{
    public void MyMethod()
    {
        lock (typeof(MyClass)) { }
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotLockOnThisOrTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotLockOnThisOrTypes)
            .WithSpan(6, 15, 6, 30);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestLockOnOtherObject()
    {
        const string testCode = @"
class MyClass
{
    private readonly object _lock = new object();

    public void MyMethod()
    {
        lock (_lock) { }
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotLockOnThisOrTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}