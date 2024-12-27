using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AbstractTypesShouldNotHavePublicConstructorsAnalyzer))]
public sealed class AbstractTypesShouldNotHavePublicConstructorsAnalyzerTests
{
    [Fact]
    public async Task TestAbstractClassWithPublicConstructor_ReportsDiagnostic()
    {
        const string testCode = @"
public abstract class MyAbstractClass
{
    public MyAbstractClass() { }
}
";

        var context = new CSharpAnalyzerTest<AbstractTypesShouldNotHavePublicConstructorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AbstractTypesShouldNotHavePublicConstructors)
            .WithSpan(4, 12, 4, 27)
            .WithArguments("MyAbstractClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAbstractClassWithoutPublicConstructor_NoDiagnostic()
    {
        const string testCode = @"
public abstract class MyAbstractClass
{
    protected MyAbstractClass() { }
}
";

        var context = new CSharpAnalyzerTest<AbstractTypesShouldNotHavePublicConstructorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestConcreteClassWithPublicConstructor_NoDiagnostic()
    {
        const string testCode = @"
public class MyConcreteClass
{
    public MyConcreteClass() { }
}
";

        var context = new CSharpAnalyzerTest<AbstractTypesShouldNotHavePublicConstructorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}