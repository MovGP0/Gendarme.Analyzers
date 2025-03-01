using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(MissingExceptionConstructorsAnalyzer))]
public sealed class MissingExceptionConstructorsAnalyzerTests
{
    [Fact]
    public async Task TestMissingConstructors()
    {
        const string testCode = @"
using System;

public class MyCustomException : Exception
{
    // No constructors defined, should trigger a diagnostic
}
";

        var context = new CSharpAnalyzerTest<MissingExceptionConstructorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MissingExceptionConstructors)
            .WithSpan(4, 14, 4, 31)
            .WithArguments("MyCustomException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestWithAllConstructors()
    {
        const string testCode = @"
using System;

public class MyCustomException : Exception
{
    public MyCustomException() : base() { }
    public MyCustomException(string message) : base(message) { }
    public MyCustomException(string message, Exception innerException) : base(message, innerException) { }
    protected MyCustomException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
        : base(info, context) { }
}
";

        var context = new CSharpAnalyzerTest<MissingExceptionConstructorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since all required constructors are implemented
        await context.RunAsync();
    }

    [Fact]
    public async Task TestMissingProtectedConstructor()
    {
        const string testCode = @"
using System;

public class MyCustomException : Exception
{
    public MyCustomException(string message) : base(message) { }
}
";

        var context = new CSharpAnalyzerTest<MissingExceptionConstructorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MissingExceptionConstructors)
            .WithSpan(4, 14, 4, 31)
            .WithArguments("MyCustomException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}