using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(AvoidThrowingBasicExceptionsAnalyzer))]
public sealed class AvoidThrowingBasicExceptionsAnalyzerTests
{
    [Fact]
    public async Task TestThrowingBasicException()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        throw new Exception();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidThrowingBasicExceptionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidThrowingBasicExceptions)
            .WithSpan(6, 9, 6, 15)
            .WithArguments("Exception");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestThrowingApplicationException()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        throw new ApplicationException();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidThrowingBasicExceptionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidThrowingBasicExceptions)
            .WithSpan(6, 9, 6, 23)
            .WithArguments("ApplicationException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestThrowingSystemException()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        throw new SystemException();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidThrowingBasicExceptionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidThrowingBasicExceptions)
            .WithSpan(6, 9, 6, 18)
            .WithArguments("SystemException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestThrowingCustomException()
    {
        const string testCode = @"
using System;

public class MyCustomException : Exception { }

public class MyClass
{
    public void MyMethod()
    {
        throw new MyCustomException();
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidThrowingBasicExceptionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for custom exceptions
        await context.RunAsync();
    }
}