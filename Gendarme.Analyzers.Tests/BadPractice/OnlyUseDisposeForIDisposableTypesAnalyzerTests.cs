using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(OnlyUseDisposeForIDisposableTypesAnalyzer))]
public sealed class OnlyUseDisposeForIDisposableTypesAnalyzerTests
{
    [Fact]
    public async Task TestDisposeMethodOnNonDisposableType()
    {
        const string testCode = @"
class NonDisposable
{
    public void Dispose() { }
}";

        var context = new CSharpAnalyzerTest<OnlyUseDisposeForIDisposableTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.OnlyUseDisposeForIDisposableTypes)
            .WithSpan(4, 17, 4, 24)
            .WithArguments("Dispose");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestDisposeMethodOnDisposableType()
    {
        const string testCode = @"
using System;

class MyDisposable : IDisposable
{
    public void Dispose() { }
}";

        var context = new CSharpAnalyzerTest<OnlyUseDisposeForIDisposableTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for this case.
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonDisposeMethodOnNonDisposableType()
    {
        const string testCode = @"
class NonDisposable
{
    public void NonDispose() { }
}";

        var context = new CSharpAnalyzerTest<OnlyUseDisposeForIDisposableTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for this case.
        await context.RunAsync();
    }
}