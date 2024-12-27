using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(UseCorrectSuffixAnalyzer))]
public sealed class UseCorrectSuffixAnalyzerTests
{
    [Fact]
    public async Task TestCorrectSuffixForAttribute()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MyCustomAttribute { }

public class MyCustomAttributeAttribute : MyCustomAttribute { }
";

        var context = new CSharpAnalyzerTest<UseCorrectSuffixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectSuffix)
            .WithSpan(7, 14, 7, 41)
            .WithArguments("MyCustomAttributeAttribute", "Attribute");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestIncorrectSuffixForEventArgs()
    {
        const string testCode = @"
using System;

public class MyEventArgs : EventArgs { }
";

        var context = new CSharpAnalyzerTest<UseCorrectSuffixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectSuffix)
            .WithSpan(5, 14, 5, 28)
            .WithArguments("MyEventArgs", "EventArgs");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoSuffixIssueForWellNamedClass()
    {
        const string testCode = @"
using System;

public class MyStream : Stream { }
";

        var context = new CSharpAnalyzerTest<UseCorrectSuffixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected because the name is correct
        await context.RunAsync();
    }
}