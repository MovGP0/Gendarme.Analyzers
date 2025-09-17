using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(TypesShouldBeInsideNamespacesAnalyzer))]
public sealed class TypesShouldBeInsideNamespacesAnalyzerTests
{
    [Fact]
    public async Task TestTypeNotInNamespace()
    {
        const string testCode = @"
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<TypesShouldBeInsideNamespacesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.TypesShouldBeInsideNamespaces)
            .WithSpan(2, 14, 2, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestTypeInNamespace()
    {
        const string testCode = @"
namespace MyNamespace
{
    public class MyClass { }
}
";

        var context = new CSharpAnalyzerTest<TypesShouldBeInsideNamespacesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected in this case
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonPublicTypeNotInNamespace()
    {
        const string testCode = @"
class MyClass { }
";

        var context = new CSharpAnalyzerTest<TypesShouldBeInsideNamespacesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for non-public types
        await context.RunAsync();
    }
}