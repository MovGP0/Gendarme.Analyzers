using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(DoNotForgetNotImplementedMethodsAnalyzer))]
public sealed class DoNotForgetNotImplementedMethodsAnalyzerTests
{
    [Fact]
    public async Task TestNotImplementedMethodDiagnostic()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        throw new NotImplementedException();
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotForgetNotImplementedMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotForgetNotImplementedMethods)
            .WithSpan(8, 9, 8, 45)
            .WithArguments("MyMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestImplementedMethodNoDiagnostic()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod()
    {
        System.Console.WriteLine(""Hello World"");
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotForgetNotImplementedMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestSkipLongMethodNoDiagnostic()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyLongMethod()
    {
        throw new NotImplementedException();
        Console.WriteLine(""This is a long method and should not trigger a diagnostic."");
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotForgetNotImplementedMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotForgetNotImplementedMethods)
            .WithSpan(8, 9, 8, 45)
            .WithArguments("MyLongMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}