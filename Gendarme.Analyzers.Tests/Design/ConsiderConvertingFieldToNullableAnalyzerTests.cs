using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ConsiderConvertingFieldToNullableAnalyzer))]
public sealed class ConsiderConvertingFieldToNullableAnalyzerTests
{
    [Fact]
    public async Task TestFieldConsideration()
    {
        const string testCode = @"
public class MyClass
{
    private bool hasValue;
    private int value;
    
    // This should trigger a warning!
    private bool hasNonNullable;
    private NonNullableType nonNullableValue;
}
";

        var context = new CSharpAnalyzerTest<ConsiderConvertingFieldToNullableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ConsiderConvertingFieldToNullable)
            .WithSpan(7, 9, 7, 29)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoTriggerForNonMatchingFields()
    {
        const string testCode = @"
public class MyClass
{
    private bool hasValue;
    private string value; // This should NOT trigger a warning
}
";

        var context = new CSharpAnalyzerTest<ConsiderConvertingFieldToNullableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // Should not report any diagnostics
    }
}