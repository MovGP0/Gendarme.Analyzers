using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUnneededCallsOnStringAnalyzer))]
public sealed class AvoidUnneededCallsOnStringAnalyzerTests
{
    [Fact]
    public async Task TestUnneededToStringCall()
    {
        const string testCode = @"
public class MyClass {
    public void TestMethod() {
        string s = ""Hello"";
        s.ToString();
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnneededCallsOnStringAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidUnneededCallsOnString, DiagnosticSeverity.Info)
            .WithSpan(5, 9, 5, 21)
            .WithArguments("ToString", "s");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestUnneededCloneCall()
    {
        const string testCode = @"
public class MyClass {
    public void TestMethod() {
        string s = ""Hello"";
        s.Clone();
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnneededCallsOnStringAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidUnneededCallsOnString, DiagnosticSeverity.Info)
            .WithSpan(5, 9, 5, 18)
            .WithArguments("Clone", "s");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestUnneededSubstringCall()
    {
        const string testCode = @"
public class MyClass {
    public void TestMethod() {
        string s = ""Hello"";
        s.Substring(1);
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnneededCallsOnStringAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostic expected for Substring with non-zero start index
        await context.RunAsync();
    }

    [Fact]
    public async Task TestUnneededSubstringCallWithZero()
    {
        const string testCode = @"
public class MyClass {
    public void TestMethod() {
        string s = ""Hello"";
        s.Substring(0);
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnneededCallsOnStringAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidUnneededCallsOnString, DiagnosticSeverity.Info)
            .WithSpan(5, 9, 5, 23)
            .WithArguments("Substring", "s");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}