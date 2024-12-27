using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ProvideCorrectArgumentsToFormattingMethodsAnalyzer))]
public sealed class ProvideCorrectArgumentsToFormattingMethodsAnalyzerTests
{
    [Fact]
    public async Task TestCorrectArgumentCount()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public void TestMethod()
    {
        string result = String.Format(""{0} {1}"", ""Hello"", ""World"");
    }
}
";

        var context = new CSharpAnalyzerTest<ProvideCorrectArgumentsToFormattingMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMismatchArgumentCount()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public void TestMethod()
    {
        string result = String.Format(""{0} {1}"", ""Hello"");
    }
}
";

        var context = new CSharpAnalyzerTest<ProvideCorrectArgumentsToFormattingMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideCorrectArgumentsToFormattingMethods)
            .WithSpan(8, 25, 8, 58)
            .WithArguments("String.Format");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestFormatStringWithNoPlaceholders()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public void TestMethod()
    {
        string result = String.Format(""Hello World"");
    }
}
";

        var context = new CSharpAnalyzerTest<ProvideCorrectArgumentsToFormattingMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}