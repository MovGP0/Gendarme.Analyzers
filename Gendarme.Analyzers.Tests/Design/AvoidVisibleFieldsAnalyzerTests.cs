using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AvoidVisibleFieldsAnalyzer))]
public sealed class AvoidVisibleFieldsAnalyzerTests
{
    [Fact]
    public async Task TestVisibleFieldWarning()
    {
        const string testCode = @"
public class MyClass
{
    public int VisibleField; // This should trigger a warning
}
";

        var context = new CSharpAnalyzerTest<AvoidVisibleFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidVisibleFields)
            .WithSpan(4, 5, 4, 19)
            .WithArguments("VisibleField");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestStaticReadOnlyFieldNoWarning()
    {
        const string testCode = @"
public class MyClass
{
    public static readonly int StaticReadOnlyField = 42; // No warning expected
}
";

        var context = new CSharpAnalyzerTest<AvoidVisibleFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestConstFieldNoWarning()
    {
        const string testCode = @"
public class MyClass
{
    public const int ConstField = 100; // No warning expected
}
";

        var context = new CSharpAnalyzerTest<AvoidVisibleFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}