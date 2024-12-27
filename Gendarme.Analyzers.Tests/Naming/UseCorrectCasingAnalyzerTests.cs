using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(UseCorrectCasingAnalyzer))]
public sealed class UseCorrectCasingAnalyzerTests
{
    [Fact]
    public async Task TestNamespaceInPascalCase()
    {
        const string testCode = @"
namespace invalid_namespace
{
    class MyClass { }
}";

        var context = new CSharpAnalyzerTest<UseCorrectCasingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectCasing)
            .WithSpan(2, 1, 2, 18)
            .WithArguments("Namespace", "invalid_namespace");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassInPascalCase()
    {
        const string testCode = @"
namespace ValidNamespace
{
    class invalidClass { }
}";

        var context = new CSharpAnalyzerTest<UseCorrectCasingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectCasing)
            .WithSpan(4, 7, 4, 18)
            .WithArguments("Type", "invalidClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodInPascalCase()
    {
        const string testCode = @"
namespace ValidNamespace
{
    class MyClass
    {
        void invalidMethod() { }
    }
}";

        var context = new CSharpAnalyzerTest<UseCorrectCasingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectCasing)
            .WithSpan(6, 10, 6, 22)
            .WithArguments("Method", "invalidMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestParameterInCamelCase()
    {
        const string testCode = @"
namespace ValidNamespace
{
    class MyClass
    {
        void MyMethod(int InvalidParameter) { }
    }
}";

        var context = new CSharpAnalyzerTest<UseCorrectCasingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectCasing)
            .WithSpan(6, 24, 6, 39)
            .WithArguments("Parameter", "InvalidParameter");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}