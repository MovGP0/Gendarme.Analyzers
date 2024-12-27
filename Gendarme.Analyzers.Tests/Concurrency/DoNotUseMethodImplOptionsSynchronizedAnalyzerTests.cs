using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotUseMethodImplOptionsSynchronizedAnalyzer))]
public sealed class DoNotUseMethodImplOptionsSynchronizedAnalyzerTests
{
    [Fact]
    public async Task TestMethodImplOptionsSynchronized()
    {
        const string testCode = @"
using System.Runtime.CompilerServices;

public class MyClass
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void MySynchronizedMethod() { }
}";

        var context = new CSharpAnalyzerTest<DoNotUseMethodImplOptionsSynchronizedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotUseMethodImplOptionsSynchronized)
            .WithSpan(7, 17, 7, 37)
            .WithArguments("MySynchronizedMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodWithoutSynchronized()
    {
        const string testCode = @"
public class MyClass
{
    public void MyNormalMethod() { }
}";

        var context = new CSharpAnalyzerTest<DoNotUseMethodImplOptionsSynchronizedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}