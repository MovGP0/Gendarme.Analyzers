using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(RemoveUnusedLocalVariablesAnalyzer))]
public sealed class RemoveUnusedLocalVariablesAnalyzerTests
{
    [Fact]
    public async Task TestUnusedLocalVariable()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod()
            {
                int unusedVariable = 42;
            }
        }";

        var context = new CSharpAnalyzerTest<RemoveUnusedLocalVariablesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.RemoveUnusedLocalVariables, DiagnosticSeverity.Info)
            .WithSpan(5, 17, 5, 34)
            .WithArguments("unusedVariable");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestUsedLocalVariable()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        int usedVariable = 42;
        Console.WriteLine(usedVariable);
    }
}";

        var context = new CSharpAnalyzerTest<RemoveUnusedLocalVariablesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for usedVariable

        await context.RunAsync();
    }
}