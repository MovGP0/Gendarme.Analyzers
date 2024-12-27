using Gendarme.Analyzers.Performance;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(PreferCharOverloadAnalyzer))]
public sealed class PreferCharOverloadAnalyzerTests
{
    [Fact]
    public async Task TestStringIndexOfWithSingleCharacter()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void Method()
    {
        var str = ""a"";
        str.IndexOf(""a"");
    }
}";

        var context = new CSharpAnalyzerTest<PreferCharOverloadAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.PreferCharOverload, DiagnosticSeverity.Info)
            .WithSpan(9, 9, 9, 25)
            .WithArguments("IndexOf", "string");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestStringStartsWithWithSingleCharacter()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void Method()
    {
        var str = ""a"";
        str.StartsWith(""a"");
    }
}";

        var context = new CSharpAnalyzerTest<PreferCharOverloadAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.PreferCharOverload, DiagnosticSeverity.Info)
            .WithSpan(9, 9, 9, 28)
            .WithArguments("StartsWith", "string");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoDiagnosticForMultiCharacterString()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void Method()
    {
        var str = ""abc"";
        str.IndexOf(""abc"");
    }
}";

        var context = new CSharpAnalyzerTest<PreferCharOverloadAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}