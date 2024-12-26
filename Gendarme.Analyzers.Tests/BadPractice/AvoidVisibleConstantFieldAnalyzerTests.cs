using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(AvoidVisibleConstantFieldAnalyzer))]
public sealed class AvoidVisibleConstantFieldAnalyzerTests
{
    [Fact]
    public async Task TestVisibleConstantField()
    {
        const string testCode = @"
public class MyClass
{
    public const int VisibleConstant = 42;
}
";

        var context = new CSharpAnalyzerTest<AvoidVisibleConstantFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidVisibleConstantField)
            .WithSpan(4, 22, 4, 37)
            .WithArguments("VisibleConstant");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonVisibleConstantField()
    {
        const string testCode = @"
public class MyClass
{
    internal const int NonVisibleConstant = 42;
}
";

        var context = new CSharpAnalyzerTest<AvoidVisibleConstantFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonConstantField()
    {
        const string testCode = @"
public class MyClass
{
    public int NonConstantField = 42;
}
";

        var context = new CSharpAnalyzerTest<AvoidVisibleConstantFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}