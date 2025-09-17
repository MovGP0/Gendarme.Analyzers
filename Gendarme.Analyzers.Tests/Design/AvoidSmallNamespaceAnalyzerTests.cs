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
}
";

        var context = new CSharpAnalyzerTest<AvoidSmallNamespaceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidSmallNamespace)
            .WithSpan(4, 18, 4, 26)
            .WithArguments("SmallNamespace", 4, 5);

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
}
";

        var context = new CSharpAnalyzerTest<AvoidSmallNamespaceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidSmallNamespace)
            .WithSpan(4, 18, 4, 26)
            .WithArguments("SmallNamespace", 2, 5);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}