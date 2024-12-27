using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidCodeDuplicatedInSiblingClassesAnalyzer))]
public sealed class AvoidCodeDuplicatedInSiblingClassesAnalyzerTests
{
    [Fact]
    public async Task TestDuplicatedMethodsInSiblingClasses()
    {
        const string testCode = @"
namespace TestNamespace
{
    public class BaseClass
    {
        public void CommonMethod() { }
    }

    public class DerivedClassA : BaseClass
    {
        public void CommonMethod() { }
    }

    public class DerivedClassB : BaseClass
    {
        public void CommonMethod() { }
    }
}";

        var context = new CSharpAnalyzerTest<AvoidCodeDuplicatedInSiblingClassesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidCodeDuplicatedInSiblingClasses)
            .WithSpan(11, 21, 11, 33);

        context.ExpectedDiagnostics.Add(expected1);

        var expected2 = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidCodeDuplicatedInSiblingClasses)
            .WithSpan(16, 21, 16, 33);

        context.ExpectedDiagnostics.Add(expected2);

        await context.RunAsync();
    }
}