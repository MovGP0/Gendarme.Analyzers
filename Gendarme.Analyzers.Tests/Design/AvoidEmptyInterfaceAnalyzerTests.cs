using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AvoidEmptyInterfaceAnalyzer))]
public sealed class AvoidEmptyInterfaceAnalyzerTests
{
    [Fact]
    public async Task TestEmptyInterface()
    {
        const string testCode = @"
using System;

public interface IMyEmptyInterface { }
";

        var context = new CSharpAnalyzerTest<AvoidEmptyInterfaceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidEmptyInterface)
            .WithSpan(4, 22, 4, 45)
            .WithArguments("IMyEmptyInterface");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonEmptyInterface()
    {
        const string testCode = @"
using System;

public interface IMyNonEmptyInterface
{
    void MyMethod();
}
";

        var context = new CSharpAnalyzerTest<AvoidEmptyInterfaceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics for non-empty interfaces
        await context.RunAsync();
    }
}