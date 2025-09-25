using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(ReplaceIncompleteOddnessCheckAnalyzer))]
public sealed class ReplaceIncompleteOddnessCheckAnalyzerTests
{
    [Fact]
    public async Task DetectsModuloComparisonEqualsOne()
    {
        const string testCode = @"using System;

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

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReplaceIncompleteOddnessCheck)
            .WithSpan(7, 13, 7, 28);

        await VerifyAsync(testCode, expected);
    }

    [Fact]
    public async Task DetectsModuloComparisonEqualsOneWithReversedOperands()
    {
        const string testCode = @"using System;

public class MyClass
{
    public void CheckOddness(int number)
    {
        if (1 == number % 2)
        {
            Console.WriteLine(""Odd"");
        }
    }
}";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReplaceIncompleteOddnessCheck)
            .WithSpan(7, 13, 7, 28);

        await VerifyAsync(testCode, expected);
    }

    [Fact]
    public async Task SkipsModuloComparisonEqualsZero()
    {
        const string testCode = @"using System;

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

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task SkipsModuloComparisonNotEqualZero()
    {
        const string testCode = @"using System;

public class MyClass
{
    public void CheckOddness(int number)
    {
        if (number % 2 != 0)
        {
            Console.WriteLine(""Odd"");
        }
    }
}";

        await VerifyAsync(testCode);
    }

    private static Task VerifyAsync(string source, params DiagnosticResult[] expectedDiagnostics)
    {
        var test = new CSharpAnalyzerTest<ReplaceIncompleteOddnessCheckAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        return test.RunAsync();
    }
}
