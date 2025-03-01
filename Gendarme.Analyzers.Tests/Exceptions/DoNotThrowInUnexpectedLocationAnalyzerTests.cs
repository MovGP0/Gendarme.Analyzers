using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(DoNotThrowInUnexpectedLocationAnalyzer))]
public sealed class DoNotThrowInUnexpectedLocationAnalyzerTests
{
    [Fact]
    public async Task TestThrowInEqualsMethod()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public override bool Equals(object obj)
    {
        throw new Exception();
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotThrowInUnexpectedLocationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotThrowInUnexpectedLocation)
            .WithSpan(8, 9, 8, 31)
            .WithArguments("Equals");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestThrowInGetHashCodeMethod()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public override int GetHashCode()
    {
        throw new Exception();
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotThrowInUnexpectedLocationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotThrowInUnexpectedLocation)
            .WithSpan(8, 9, 8, 31)
            .WithArguments("GetHashCode");

        context.ExpectedDiagnostics.Add(expected);
        
        await context.RunAsync();
    }

    [Fact]
    public async Task TestThrowInToStringMethod()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public override string ToString()
    {
        throw new Exception();
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotThrowInUnexpectedLocationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotThrowInUnexpectedLocation)
            .WithSpan(8, 9, 8, 31)
            .WithArguments("ToString");

        context.ExpectedDiagnostics.Add(expected);
        
        await context.RunAsync();
    }

    [Fact]
    public async Task TestThrowInFinalizeMethod()
    {
        const string testCode = @"
using System;

public class MyClass
{
    ~MyClass()
    {
        throw new Exception();
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotThrowInUnexpectedLocationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotThrowInUnexpectedLocation)
            .WithSpan(8, 9, 8, 31)
            .WithArguments("Finalize");

        context.ExpectedDiagnostics.Add(expected);
        
        await context.RunAsync();
    }

    [Fact]
    public async Task TestThrowInDisposeMethod()
    {
        const string testCode = @"
using System;

public class MyClass : IDisposable
{
    public void Dispose()
    {
        throw new Exception();
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotThrowInUnexpectedLocationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotThrowInUnexpectedLocation)
            .WithSpan(8, 9, 8, 31)
            .WithArguments("Dispose");

        context.ExpectedDiagnostics.Add(expected);
        
        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidThrowLocations()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void SomeMethod()
    {
        throw new Exception(); // This is allowed.
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotThrowInUnexpectedLocationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}