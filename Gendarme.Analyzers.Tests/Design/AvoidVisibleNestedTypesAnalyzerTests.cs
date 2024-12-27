using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AvoidVisibleNestedTypesAnalyzer))]
public sealed class AvoidVisibleNestedTypesAnalyzerTests
{
    [Fact]
    public async Task TestVisibleNestedType()
    {
        const string testCode = @"
namespace TestNamespace
{
    public class OuterClass
    {
        public class VisibleNestedClass { }
    }
}";

        var context = new CSharpAnalyzerTest<AvoidVisibleNestedTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidVisibleNestedTypes)
            .WithSpan(6, 14, 6, 33)
            .WithArguments("VisibleNestedClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvisibleNestedType()
    {
        const string testCode = @"
namespace TestNamespace
{
    public class OuterClass
    {
        internal class InvisibleNestedClass { }
    }
}";

        var context = new CSharpAnalyzerTest<AvoidVisibleNestedTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for internal nested classes
        await context.RunAsync();
    }
}