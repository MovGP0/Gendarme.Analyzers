using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(EnumsShouldDefineAZeroValueAnalyzer))]
public sealed class EnumsShouldDefineAZeroValueAnalyzerTests
{
    [Fact]
    public async Task TestEnumWithoutZeroValue()
    {
        const string testCode = @"
public enum MyEnum
{
    One = 1,
    Two = 2
}";

        var context = new CSharpAnalyzerTest<EnumsShouldDefineAZeroValueAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.EnumsShouldDefineAZeroValue)
            .WithSpan(3, 6, 3, 12)
            .WithArguments("MyEnum");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEnumWithZeroValue()
    {
        const string testCode = @"
public enum MyEnum
{
    Zero = 0,
    One = 1,
    Two = 2
}";

        var context = new CSharpAnalyzerTest<EnumsShouldDefineAZeroValueAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestFlagsEnumWithoutZeroValue()
    {
        const string testCode = @"
[System.Flags]
public enum MyFlagsEnum
{
    None = 0,
    One = 1,
    Two = 2
}";

        var context = new CSharpAnalyzerTest<EnumsShouldDefineAZeroValueAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since it's a flags enum
        await context.RunAsync();
    }
}