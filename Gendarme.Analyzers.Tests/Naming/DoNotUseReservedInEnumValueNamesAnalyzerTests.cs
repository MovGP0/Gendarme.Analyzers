using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(DoNotUseReservedInEnumValueNamesAnalyzer))]
public sealed class DoNotUseReservedInEnumValueNamesAnalyzerTests
{
    [Fact]
    public async Task TestReservedEnumValueName()
    {
        const string testCode = @"
enum MyEnum
{
    Reserved,
    RegularValue
}";

        var context = new CSharpAnalyzerTest<DoNotUseReservedInEnumValueNamesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotUseReservedInEnumValueNames)
            .WithSpan(3, 5, 3, 12)
            .WithArguments("Reserved", "MyEnum");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonReservedEnumValueName()
    {
        const string testCode = @"
enum MyEnum
{
    RegularValue,
    AnotherValue
}";

        var context = new CSharpAnalyzerTest<DoNotUseReservedInEnumValueNamesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}