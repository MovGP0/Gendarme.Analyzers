using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(DoNotDeclareProtectedMembersInSealedTypeAnalyzer))]
public sealed class DoNotDeclareProtectedMembersInSealedTypeAnalyzerTests
{
    [Fact]
    public async Task TestProtectedMemberInSealedType()
    {
        const string testCode = @"
public sealed class MySealedClass
{
    protected int MyProtectedField; // should trigger a warning
    protected void MyProtectedMethod() { } // should trigger a warning
}
";

        var context = new CSharpAnalyzerTest<DoNotDeclareProtectedMembersInSealedTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expectedFieldWarning = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotDeclareProtectedMembersInSealedType)
            .WithSpan(4, 19, 4, 35)
            .WithArguments("MySealedClass", "MyProtectedField");

        var expectedMethodWarning = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotDeclareProtectedMembersInSealedType)
            .WithSpan(5, 20, 5, 37)
            .WithArguments("MySealedClass", "MyProtectedMethod");

        context.ExpectedDiagnostics.Add(expectedFieldWarning);
        context.ExpectedDiagnostics.Add(expectedMethodWarning);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoProtectedMembersInSealedType()
    {
        const string testCode = @"
public sealed class MySealedClass
{
    public int MyPublicField; // does not trigger a warning
    private void MyPrivateMethod() { } // does not trigger a warning
}
";

        var context = new CSharpAnalyzerTest<DoNotDeclareProtectedMembersInSealedTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No warnings expected
        await context.RunAsync();
    }
}