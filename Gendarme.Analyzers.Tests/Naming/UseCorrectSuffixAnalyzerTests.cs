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
public class MyCustomAttribute: Attribute { }

public class MyCustomClass : MyCustomAttribute { }
";

        var context = new CSharpAnalyzerTest<UseCorrectSuffixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectSuffix)
            .WithSpan(7, 14, 7, 27)
            .WithArguments("MyCustomClass", "Attribute", "Attribute");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestIncorrectSuffixForEventArgs()
    {
        const string testCode = @"
using System;

public class MyEvent : EventArgs { }
";

        var context = new CSharpAnalyzerTest<UseCorrectSuffixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectSuffix)
            .WithSpan(4, 14, 4, 21)
            .WithArguments("MyEvent", "EventArgs", "EventArgs");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoSuffixIssueForWellNamedClass()
    {
        const string testCode = @"
using System.IO;

public abstract class MyStream : Stream { }
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