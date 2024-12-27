using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(OverrideEqualsMethodAnalyzer))]
public sealed class OverrideEqualsMethodAnalyzerTests
{
    [Fact]
    public async Task TestOperatorEqualityWithoutEquals()
    {
        const string testCode = @"
public class MyClass
{
    public static bool operator ==(MyClass left, MyClass right) => true;
    public static bool operator !=(MyClass left, MyClass right) => false;
}
";

        var context = new CSharpAnalyzerTest<OverrideEqualsMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.OverrideEqualsMethod)
            .WithSpan(4, 5, 4, 11) // Adjust line/column based on your actual code
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestOperatorEqualityWithEquals()
    {
        const string testCode = @"
public class MyClass
{
    public static bool operator ==(MyClass left, MyClass right) => true;
    public static bool operator !=(MyClass left, MyClass right) => false;

    public override bool Equals(object obj) => true;
}
";

        var context = new CSharpAnalyzerTest<OverrideEqualsMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since Equals is overridden
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoOperatorEquality()
    {
        const string testCode = @"
public class MyClass
{
    // No operator overloads and no Equals override
}
";

        var context = new CSharpAnalyzerTest<OverrideEqualsMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since there's no operator overload
        await context.RunAsync();
    }
}