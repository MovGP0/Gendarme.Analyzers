using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AvoidSmallNamespaceAnalyzer))]
public sealed class AvoidSmallNamespaceAnalyzerTests
{
    [Fact]
    public async Task TestSmallNamespace()
    {
        const string testCode = @"
namespace SmallNamespace
{
    public class MyClass1 { }
    public class MyClass2 { }
    public class MyClass3 { }
    public class MyClass4 { }
    public class MyClass5 { }
    public class MyClass6 { }
}";

        var context = new CSharpAnalyzerTest<AvoidSmallNamespaceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidSmallNamespace)
            .WithSpan(4, 13, 4, 21)
            .WithArguments("SmallNamespace", 6, 5);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestTooFewTypesInNamespace()
    {
        const string testCode = @"
namespace SmallNamespace
{
    public class MyClass1 { }
    public class MyClass2 { }
}";

        var context = new CSharpAnalyzerTest<AvoidSmallNamespaceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidSmallNamespace)
            .WithSpan(4, 13, 4, 21)
            .WithArguments("SmallNamespace", 2, 5);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}