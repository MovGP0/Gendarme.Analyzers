using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(AvoidDeepNamespaceHierarchyAnalyzer))]
public sealed class AvoidDeepNamespaceHierarchyAnalyzerTests
{
    [Fact]
    public async Task TestDeepNamespaceHierarchy()
    {
        const string testCode = @"
namespace Gendarme.Analyzers.Naming.Deep.Deep.Deep.Namespace
{
    public class MyClass { }
}";

        var context = new CSharpAnalyzerTest<AvoidDeepNamespaceHierarchyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidDeepNamespaceHierarchy)
            .WithSpan(2, 17, 2, 50)
            .WithArguments("Gendarme.Analyzers.Naming.Deep.Deep.Deep.Namespace", 5);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAllowedNamespaceDepth()
    {
        const string testCode = @"
namespace Gendarme.Analyzers.Naming.Deep.Design
{
    public class MyClass { }
}";

        var context = new CSharpAnalyzerTest<AvoidDeepNamespaceHierarchyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNamespaceWithUnderscore()
    {
        const string testCode = @"
namespace Gendarme.Analyzers.Naming._DeepNamespace
{
    public class MyClass { }
}";

        var context = new CSharpAnalyzerTest<AvoidDeepNamespaceHierarchyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}