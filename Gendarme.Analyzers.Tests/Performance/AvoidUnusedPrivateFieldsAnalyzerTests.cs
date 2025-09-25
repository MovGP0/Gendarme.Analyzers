using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUnusedPrivateFieldsAnalyzer))]
public sealed class AvoidUnusedPrivateFieldsAnalyzerTests
{
    [Fact]
    public async Task TestUnusedPrivateField()
    {
        const string testCode = @"
public class MyClass
{
    private int unusedField;

    public void UsedMethod()
    {
        // This method does not use the unusedField
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnusedPrivateFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidUnusedPrivateFields, DiagnosticSeverity.Info)
            .WithSpan(4, 14, 4, 25)
            .WithArguments("unusedField");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestUsedPrivateField()
    {
        const string testCode = @"
public class MyClass
{
    private int usedField;

    public void UsedMethod()
    {
        usedField = 5; // This method uses the usedField
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnusedPrivateFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected, since usedField is used in UsedMethod.
        await context.RunAsync();
    }
}