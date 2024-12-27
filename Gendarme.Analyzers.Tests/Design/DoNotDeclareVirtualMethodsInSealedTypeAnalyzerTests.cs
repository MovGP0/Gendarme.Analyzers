using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(DoNotDeclareVirtualMethodsInSealedTypeAnalyzer))]
public sealed class DoNotDeclareVirtualMethodsInSealedTypeAnalyzerTests
{
    [Fact]
    public async Task TestSealedTypeWithVirtualMethod()
    {
        const string testCode = @"
sealed class SealedClass
{
    public virtual void MyMethod() { }
}";

        var context = new CSharpAnalyzerTest<DoNotDeclareVirtualMethodsInSealedTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotDeclareVirtualMethodsInSealedType)
            .WithSpan(3, 17, 3, 27)
            .WithArguments("SealedClass", "MyMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestSealedTypeWithNoVirtualMethod()
    {
        const string testCode = @"
sealed class SealedClass
{
    public void MyMethod() { }
}";

        var context = new CSharpAnalyzerTest<DoNotDeclareVirtualMethodsInSealedTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}