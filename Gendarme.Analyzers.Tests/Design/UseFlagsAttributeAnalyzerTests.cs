using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(UseFlagsAttributeAnalyzer))]
public sealed class UseFlagsAttributeAnalyzerTests
{
    [Fact]
    public async Task TestEnumWithoutFlagsAttribute_HasMultiplePowerOfTwoFields()
    {
        const string testCode = @"
public enum MyFlags
{
    None = 0,
    OptionA = 1,
    OptionB = 2,
    OptionC = 4
}
";

        var context = new CSharpAnalyzerTest<UseFlagsAttributeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseFlagsAttribute)
            .WithSpan(2, 14, 2, 21)
            .WithArguments("MyFlags");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEnumWithoutFlagsAttribute_SinglePowerOfTwoField()
    {
        const string testCode = @"
public enum MyFlags
{
    None = 0,
    OptionA = 1
}
";

        var context = new CSharpAnalyzerTest<UseFlagsAttributeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestEnumWithFlagsAttribute()
    {
        const string testCode = @"
[System.Flags]
public enum MyFlags
{
    None = 0,
    OptionA = 1,
    OptionB = 2
}
";

        var context = new CSharpAnalyzerTest<UseFlagsAttributeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonEnumType()
    {
        const string testCode = @"
public class MyClass
{
    public int Value { get; set; }
}
";

        var context = new CSharpAnalyzerTest<UseFlagsAttributeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}