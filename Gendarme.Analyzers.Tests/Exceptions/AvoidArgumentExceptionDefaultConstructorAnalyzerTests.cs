using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(AvoidArgumentExceptionDefaultConstructorAnalyzer))]
public sealed class AvoidArgumentExceptionDefaultConstructorAnalyzerTests
{
    [Fact]
    public async Task TestArgumentExceptionDefaultConstructor()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public MyClass()
    {
        throw new ArgumentException();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidArgumentExceptionDefaultConstructorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidArgumentExceptionDefaultConstructor)
            .WithSpan(6, 14, 6, 31)
            .WithArguments("ArgumentException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestArgumentNullExceptionDefaultConstructor()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public MyClass()
    {
        throw new ArgumentNullException();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidArgumentExceptionDefaultConstructorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidArgumentExceptionDefaultConstructor)
            .WithSpan(6, 14, 6, 31)
            .WithArguments("ArgumentNullException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestArgumentOutOfRangeExceptionDefaultConstructor()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public MyClass()
    {
        throw new ArgumentOutOfRangeException();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidArgumentExceptionDefaultConstructorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidArgumentExceptionDefaultConstructor)
            .WithSpan(6, 14, 6, 41)
            .WithArguments("ArgumentOutOfRangeException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoDiagnosticForCustomException()
    {
        const string testCode = @"
public class CustomException : System.Exception
{
    public CustomException() { }
}

public class MyClass
{
    public MyClass()
    {
        throw new CustomException();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidArgumentExceptionDefaultConstructorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}