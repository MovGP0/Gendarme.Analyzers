using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(InternalNamespacesShouldNotExposeTypesAnalyzer))]
public sealed class InternalNamespacesShouldNotExposeTypesAnalyzerTests
{
    [Fact]
    public async Task TestPublicClassInInternalNamespace()
    {
        const string testCode = @"
namespace Gendarme.Analyzers.Internal
{
    public class MyClass { }
}";

        var context = new CSharpAnalyzerTest<InternalNamespacesShouldNotExposeTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.InternalNamespacesShouldNotExposeTypes)
            .WithSpan(4, 14, 4, 21)
            .WithArguments("MyClass", "Gendarme.Analyzers.Internal");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPublicClassInNormalNamespace()
    {
        const string testCode = @"
namespace Gendarme.Analyzers
{
    public class MyClass { }
}";

        var context = new CSharpAnalyzerTest<InternalNamespacesShouldNotExposeTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
        Assert.Empty(context.ExpectedDiagnostics);
    }

    [Fact]
    public async Task TestPublicClassInImplNamespace()
    {
        const string testCode = @"
namespace Gendarme.Analyzers.Impl
{
    public class MyClass { }
}";

        var context = new CSharpAnalyzerTest<InternalNamespacesShouldNotExposeTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.InternalNamespacesShouldNotExposeTypes)
            .WithSpan(4, 14, 4, 21)
            .WithArguments("MyClass", "Gendarme.Analyzers.Impl");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}