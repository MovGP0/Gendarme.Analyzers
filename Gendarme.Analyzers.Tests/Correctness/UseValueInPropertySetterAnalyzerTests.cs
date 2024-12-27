using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(UseValueInPropertySetterAnalyzer))]
public sealed class UseValueInPropertySetterAnalyzerTests
{
    [Fact]
    public async Task TestPropertySetterDoesNotUseValue()
    {
        const string testCode = @"
public class MyClass
{
    private int _field;
    public int MyProperty
    {
        set
        {
            _field = 0; // Warning expected here
        }
    }
}";

        var context = new CSharpAnalyzerTest<UseValueInPropertySetterAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseValueInPropertySetter)
            .WithSpan(7, 9, 10, 10); // Adjust for your specific method location

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPropertySetterUsesValue()
    {
        const string testCode = @"
public class MyClass
{
    private int _field;
    public int MyProperty
    {
        set
        {
            _field = value; // No warning expected here
        }
    }
}";

        var context = new CSharpAnalyzerTest<UseValueInPropertySetterAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}