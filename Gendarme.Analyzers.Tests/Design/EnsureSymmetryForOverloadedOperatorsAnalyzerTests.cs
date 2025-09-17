using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(EnsureSymmetryForOverloadedOperatorsAnalyzer))]
public sealed class EnsureSymmetryForOverloadedOperatorsAnalyzerTests
{
    [Fact]
    public async Task TestOperatorSymmetry_AdditionWithoutSubtraction()
    {
        const string testCode = @"
public class MyClass
{
    public static MyClass operator +(MyClass a, MyClass b) => new MyClass();
}
";

        var context = new CSharpAnalyzerTest<EnsureSymmetryForOverloadedOperatorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.EnsureSymmetryForOverloadedOperators)
            .WithSpan(4, 36, 4, 37)
            .WithArguments("MyClass", "+");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestOperatorSymmetry_SubtractionWithoutAddition()
    {
        const string testCode = @"
public class MyClass
{
    public static MyClass operator -(MyClass a, MyClass b) => new MyClass();
}
";

        var context = new CSharpAnalyzerTest<EnsureSymmetryForOverloadedOperatorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.EnsureSymmetryForOverloadedOperators)
            .WithSpan(4, 36, 4, 37)
            .WithArguments("MyClass", "-");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestOperatorSymmetry_BothOperatorsDefined()
    {
        const string testCode = @"
public class MyClass
{
    public static MyClass operator +(MyClass a, MyClass b) => new MyClass();
    public static MyClass operator -(MyClass a, MyClass b) => new MyClass();
}
";

        var context = new CSharpAnalyzerTest<EnsureSymmetryForOverloadedOperatorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}