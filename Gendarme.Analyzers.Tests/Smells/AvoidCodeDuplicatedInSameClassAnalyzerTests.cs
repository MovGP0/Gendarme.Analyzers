using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidCodeDuplicatedInSameClassAnalyzer))]
public sealed class AvoidCodeDuplicatedInSameClassAnalyzerTests
{
    [Fact]
    public async Task TestCodeDuplicationWarning()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MethodA()
    {
        Console.WriteLine(""Hello World"");
    }

    public void MethodB()
    {
        Console.WriteLine(""Hello World"");
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidCodeDuplicatedInSameClassAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidCodeDuplicatedInSameClass)
            .WithSpan(4, 14, 4, 21);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}