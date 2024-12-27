using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(UseCorrectDisposeSignaturesAnalyzer))]
public sealed class UseCorrectDisposeSignaturesAnalyzerTests
{
    [Fact]
    public async Task TestMissingPublicDispose()
    {
        const string testCode = @"
using System;

public class MyClass : IDisposable
{
    public void Dispose() { }
}
";

        var context = new CSharpAnalyzerTest<UseCorrectDisposeSignaturesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectDisposeSignatures)
            .WithSpan(6, 18, 6, 24)
            .WithArguments("MyClass", "[Missing public Dispose()]; ");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMissingProtectedVirtualDispose()
    {
        const string testCode = @"
using System;

public class MyClass : IDisposable
{
    public virtual void Dispose(bool disposing) { }
}
";

        var context = new CSharpAnalyzerTest<UseCorrectDisposeSignaturesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectDisposeSignatures)
            .WithSpan(6, 18, 6, 36)
            .WithArguments("MyClass", "[Missing protected virtual Dispose(bool)]; ");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCorrectDisposeSignatures()
    {
        const string testCode = @"
using System;

public class MyClass : IDisposable
{
    public void Dispose() { }
    
    protected virtual void Dispose(bool disposing) { }
}
";

        var context = new CSharpAnalyzerTest<UseCorrectDisposeSignaturesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since Dispose signatures are correct

        await context.RunAsync();
    }
}