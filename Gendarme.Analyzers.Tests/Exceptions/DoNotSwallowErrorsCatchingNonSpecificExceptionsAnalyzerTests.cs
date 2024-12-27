using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(DoNotSwallowErrorsCatchingNonSpecificExceptionsAnalyzer))]
public sealed class DoNotSwallowErrorsCatchingNonSpecificExceptionsAnalyzerTests
{
    [Fact]
    public async Task TestCatchClauseWithoutThrow()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public void Method()
    {
        try
        {
            // Some code that might throw
        }
        catch (Exception ex) { } // Catching non-specific exception without rethrowing
    }
}";

        var context = new CSharpAnalyzerTest<DoNotSwallowErrorsCatchingNonSpecificExceptionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult.CompilerWarning(DiagnosticId.DoNotSwallowErrorsCatchingNonSpecificExceptions)
            .WithSpan(7, 14, 7, 35); // Adjust the span to the location of the catch clause

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCatchClauseWithThrow()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public void Method()
    {
        try
        {
            // Some code that might throw
        }
        catch (Exception ex) 
        {
            throw; // Rethrowing the caught exception
        }
    }
}";

        var context = new CSharpAnalyzerTest<DoNotSwallowErrorsCatchingNonSpecificExceptionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected as the exception is being rethrown.
        await context.RunAsync();
    }
}