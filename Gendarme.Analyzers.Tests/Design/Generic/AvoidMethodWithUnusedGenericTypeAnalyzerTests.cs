using Gendarme.Analyzers.Design.Generic;

namespace Gendarme.Analyzers.Tests.Design.Generic;

[TestOf(typeof(AvoidMethodWithUnusedGenericTypeAnalyzer))]
public sealed class AvoidMethodWithUnusedGenericTypeAnalyzerTests
{
    [Fact]
    public async Task TestUnusedGenericType()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod<T>()
    {
        // Do something
    }

    public void MyOtherMethod<T>(int x)
    {
        // x is used but T is not
    }
}";

        var context = new CSharpAnalyzerTest<AvoidMethodWithUnusedGenericTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected0 = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidMethodWithUnusedGenericType)
            .WithSpan(4, 26, 4, 27)
            .WithArguments("T");

        context.ExpectedDiagnostics.Add(expected0);

        var expected1 = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidMethodWithUnusedGenericType)
            .WithSpan(9, 31, 9, 32)
            .WithArguments("T");

        context.ExpectedDiagnostics.Add(expected1);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestUsedGenericType_NoDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod<T>(T parameter)
    {
        // T is used
    }
}";

        var context = new CSharpAnalyzerTest<AvoidMethodWithUnusedGenericTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics for this test case
        await context.RunAsync();
    }
}