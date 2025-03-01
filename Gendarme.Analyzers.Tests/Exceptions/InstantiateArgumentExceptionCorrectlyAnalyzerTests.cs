using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(InstantiateArgumentExceptionCorrectlyAnalyzer))]
public sealed class InstantiateArgumentExceptionCorrectlyAnalyzerTests
{
    [Fact]
    public async Task TestArgumentExceptionInstantiation()
    {
        const string testCode = @"
using System;

public class TestClass 
{
    public void TestMethod()
    {
        throw new ArgumentException(""Invalid argument"");
    }
}";

        var context = new CSharpAnalyzerTest<InstantiateArgumentExceptionCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.InstantiateArgumentExceptionCorrectly)
            .WithSpan(8, 15, 8, 56)
            .WithArguments("ArgumentException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestArgumentNullExceptionInstantiation()
    {
        const string testCode = @"
using System;

public class TestClass 
{
    public void TestMethod()
    {
        throw new ArgumentNullException();
    }
}";

        var context = new CSharpAnalyzerTest<InstantiateArgumentExceptionCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.InstantiateArgumentExceptionCorrectly)
            .WithSpan(8, 15, 8, 42)
            .WithArguments("ArgumentNullException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestArgumentOutOfRangeExceptionInstantiation()
    {
        const string testCode = @"
using System;

public class TestClass 
{
    public void TestMethod()
    {
        throw new ArgumentOutOfRangeException();
    }
}";

        var context = new CSharpAnalyzerTest<InstantiateArgumentExceptionCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.InstantiateArgumentExceptionCorrectly)
            .WithSpan(8, 15, 8, 48)
            .WithArguments("ArgumentOutOfRangeException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoDiagnosticsForOtherExceptions()
    {
        const string testCode = @"
using System;

public class TestClass 
{
    public void TestMethod()
    {
        throw new InvalidOperationException(""Invalid operation"");
    }
}";

        var context = new CSharpAnalyzerTest<InstantiateArgumentExceptionCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for InvalidOperationException
        await context.RunAsync();
    }
}