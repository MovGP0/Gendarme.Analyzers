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
        // Expect the compiler error about virtual members in sealed types
        var compilerError = DiagnosticResult
            .CompilerError("CS0549")
            .WithSpan(4, 25, 4, 33)
            .WithArguments("SealedClass.MyMethod()", "SealedClass");
        context.ExpectedDiagnostics.Add(compilerError);

        // Expect our analyzer warning
        var analyzerWarning = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotDeclareVirtualMethodsInSealedType)
            .WithSpan(4, 25, 4, 33)
            .WithArguments("SealedClass", "MyMethod");
        context.ExpectedDiagnostics.Add(analyzerWarning);

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