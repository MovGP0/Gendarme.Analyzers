using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(DisableDebuggingCodeAnalyzer))]
public sealed class DisableDebuggingCodeAnalyzerTests
{
    [Fact]
    public async Task TestConsoleWriteLine()
    {
        const string testCode = @"
using System;

public class MyClass 
{
    public void SomeMethod()
    {
        Console.WriteLine(""Hello, World!"");
    }
}";

        var context = new CSharpAnalyzerTest<DisableDebuggingCodeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DisableDebuggingCode)
            .WithSpan(8, 9, 8, 26)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}