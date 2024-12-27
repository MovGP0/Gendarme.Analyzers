using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ImplementIComparableCorrectlyAnalyzer))]
public sealed class ImplementIComparableCorrectlyAnalyzerTests
{
    [Fact]
    public async Task TestMissingEqualsOperator()
    {
        const string testCode = @"
public class MyClass : System.IComparable
{
    public int CompareTo(object obj) => 0;
}
";

        var context = new CSharpAnalyzerTest<ImplementIComparableCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementIComparableCorrectly)
            .WithSpan(3, 1, 3, 32)
            .WithArguments("MyClass", "Equals(object), operator ==, operator !=");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAllOperatorsImplemented()
    {
        const string testCode = @"
public class MyClass : System.IComparable
{
    public int CompareTo(object obj) => 0;

    public override bool Equals(object obj) => true;
    public static bool operator ==(MyClass a, MyClass b) => true;
    public static bool operator !=(MyClass a, MyClass b) => false;
    public static bool operator <(MyClass a, MyClass b) => false;
    public static bool operator >(MyClass a, MyClass b) => true;
}
";

        var context = new CSharpAnalyzerTest<ImplementIComparableCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        context.ExpectedDiagnostics.Clear(); // No diagnostics expected

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMissingOperatorOverloads()
    {
        const string testCode = @"
public class MyClass : System.IComparable
{
    public int CompareTo(object obj) => 0;
    public override bool Equals(object obj) => true;
}
";

        var context = new CSharpAnalyzerTest<ImplementIComparableCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementIComparableCorrectly)
            .WithSpan(3, 1, 3, 32)
            .WithArguments("MyClass", "operator ==, operator !=");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}