using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(PreferLiteralOverInitOnlyFieldsAnalyzer))]
public sealed class PreferLiteralOverInitOnlyFieldsAnalyzerTests
{
    [Fact]
    public async Task TestStaticReadonlyFieldWithCompileTimeConstant()
    {
        const string testCode = @"
public class MyClass
{
    public static readonly int ConstantValue = 42;
}";

        var context = new CSharpAnalyzerTest<PreferLiteralOverInitOnlyFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferLiteralOverInitOnlyFields)
            .WithSpan(4, 22, 4, 37) // Adjusted positions based on actual code
            .WithArguments("ConstantValue");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestStaticReadonlyFieldWithNonConstantValue()
    {
        const string testCode = @"
public class MyClass
{
    public static readonly int NonConstantValue = GetValue();

    private static int GetValue()
    {
        return 42;
    }
}";

        var context = new CSharpAnalyzerTest<PreferLiteralOverInitOnlyFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestInstanceReadonlyFieldWithCompileTimeConstant()
    {
        const string testCode = @"
public class MyClass
{
    public readonly int InstanceConstantValue = 42;
}";

        var context = new CSharpAnalyzerTest<PreferLiteralOverInitOnlyFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestStaticFieldNotReadonly()
    {
        const string testCode = @"
public class MyClass
{
    public static int NotReadonly = 42;
}";

        var context = new CSharpAnalyzerTest<PreferLiteralOverInitOnlyFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}