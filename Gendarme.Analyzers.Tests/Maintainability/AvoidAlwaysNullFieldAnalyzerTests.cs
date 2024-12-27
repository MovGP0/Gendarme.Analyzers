using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidAlwaysNullFieldAnalyzer))]
public sealed class AvoidAlwaysNullFieldAnalyzerTests
{
    [Fact]
    public async Task TestAlwaysNullField()
    {
        const string testCode = @"
public class MyClass
{
    private string _field; // Always null
}
";

        var context = new CSharpAnalyzerTest<AvoidAlwaysNullFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidAlwaysNullField)
            .WithSpan(4, 20, 4, 26)
            .WithArguments("_field");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestAssignedFieldDoesNotTriggerWarning()
    {
        const string testCode = @"
public class MyClass
{
    private string _field = ""value""; // Assigned field
}
";

        var context = new CSharpAnalyzerTest<AvoidAlwaysNullFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since the field is assigned

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPublicFieldDoesNotTriggerWarning()
    {
        const string testCode = @"
public class MyClass
{
    public string _field; // Public field
}
";

        var context = new CSharpAnalyzerTest<AvoidAlwaysNullFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since the field is public

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValueTypeFieldDoesNotTriggerWarning()
    {
        const string testCode = @"
public class MyClass
{
    private int _field; // Value type field
}
";

        var context = new CSharpAnalyzerTest<AvoidAlwaysNullFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since the field is a value type

        await context.RunAsync();
    }
}