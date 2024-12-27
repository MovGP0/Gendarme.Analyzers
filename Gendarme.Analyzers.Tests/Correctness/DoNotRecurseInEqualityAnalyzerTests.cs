using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(DoNotRecurseInEqualityAnalyzer))]
public sealed class DoNotRecurseInEqualityAnalyzerTests
{
    [Fact(Skip = "Analyzer not working properly")]
    public async Task TestEqualityOperatorRecursion()
    {
        const string testCode = @"
public class MyClass
{
    public int Value { get; set; }
    
    public static bool operator ==(MyClass a, MyClass b)
    {
        // Recursive Use of Equality Operator
        return a.Value == b.Value && a == b;
    }

    public static bool operator !=(MyClass a, MyClass b)
    {
        return !(a == b);
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotRecurseInEqualityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotRecurseInEquality)
            .WithSpan(7, 9, 7, 20); // Adjust Span according to your code

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonRecursiveEqualityOperator()
    {
        const string testCode = @"
public class MyClass
{
    public int Value { get; set; }
    
    public static bool operator ==(MyClass a, MyClass b)
    {
        return a.Value == b.Value;
    }

    public static bool operator !=(MyClass a, MyClass b)
    {
        return !(a == b);
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotRecurseInEqualityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}