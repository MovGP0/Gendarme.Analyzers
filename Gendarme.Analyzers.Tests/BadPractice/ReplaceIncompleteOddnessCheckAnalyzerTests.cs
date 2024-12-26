using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(ReplaceIncompleteOddnessCheckAnalyzer))]
public sealed class ReplaceIncompleteOddnessCheckAnalyzerTests
{
    [Fact(Skip = "Analyzer not working yet")]
    public async Task TestIncompleteOddnessCheck()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void CheckOddness(int number)
    {
        if (number % 2 == 1) 
        {
            Console.WriteLine(""Odd"");
        }
    }
}";

        var context = new CSharpAnalyzerTest<ReplaceIncompleteOddnessCheckAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReplaceIncompleteOddnessCheck)
            .WithSpan(6, 9, 6, 22);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestValidExpression_NoDiagnostic()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void CheckOddness(int number)
    {
        if (number % 2 == 0) 
        {
            Console.WriteLine(""Even"");
        }
    }
}";

        var context = new CSharpAnalyzerTest<ReplaceIncompleteOddnessCheckAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}