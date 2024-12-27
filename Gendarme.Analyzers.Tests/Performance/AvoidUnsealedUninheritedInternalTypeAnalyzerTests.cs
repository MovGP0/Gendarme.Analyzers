using Gendarme.Analyzers.Performance;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUnsealedUninheritedInternalTypeAnalyzer))]
public sealed class AvoidUnsealedUninheritedInternalTypeAnalyzerTests
{
    [Fact]
    public async Task TestUnsealedUninheritedInternalType()
    {
        const string testCode = @"
namespace TestNamespace
{
    internal class UnsealedClass { }
}
";

        var context = new CSharpAnalyzerTest<AvoidUnsealedUninheritedInternalTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidUnsealedUninheritedInternalType, DiagnosticSeverity.Info)
            .WithSpan(4, 20, 4, 33)
            .WithArguments("UnsealedClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestSealedInternalTypeDoesNotReport()
    {
        const string testCode = @"
namespace TestNamespace
{
    internal sealed class SealedClass { }
}
";

        var context = new CSharpAnalyzerTest<AvoidUnsealedUninheritedInternalTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestInheritedInternalType()
    {
        const string testCode = @"
namespace TestNamespace
{
    internal class BaseClass { }
    internal class DerivedClass : BaseClass { }
}
";

        var context = new CSharpAnalyzerTest<AvoidUnsealedUninheritedInternalTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var diagnostic1 = new DiagnosticResult(DiagnosticId.AvoidUnsealedUninheritedInternalType, DiagnosticSeverity.Info)
            .WithSpan(4, 20, 4, 29)
            .WithArguments("BaseClass");

        context.ExpectedDiagnostics.Add(diagnostic1);

        var diagnostic2 = new DiagnosticResult(DiagnosticId.AvoidUnsealedUninheritedInternalType, DiagnosticSeverity.Info)
            .WithSpan(5, 20, 5, 32)
            .WithArguments("DerivedClass");

        context.ExpectedDiagnostics.Add(diagnostic2);

        await context.RunAsync();
    }
}