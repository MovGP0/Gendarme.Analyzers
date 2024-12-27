using Gendarme.Analyzers.Naming;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(DoNotPrefixValuesWithEnumNameAnalyzer))]
public sealed class DoNotPrefixValuesWithEnumNameAnalyzerTests
{
    [Fact]
    public async Task TestEnumNamePrefix()
    {
        const string testCode = @"
public enum MyEnum
{
    MyEnumValue1,
    MyEnumValue2,
    ValueWithoutPrefix
}
";
        var context = new CSharpAnalyzerTest<DoNotPrefixValuesWithEnumNameAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = new DiagnosticResult(DiagnosticId.DoNotPrefixValuesWithEnumName, DiagnosticSeverity.Info)
            .WithSpan(6, 5, 6, 20)
            .WithArguments("MyEnumValue1", "MyEnum");

        var expected2 = new DiagnosticResult(DiagnosticId.DoNotPrefixValuesWithEnumName, DiagnosticSeverity.Info)
            .WithSpan(7, 5, 7, 20)
            .WithArguments("MyEnumValue2", "MyEnum");

        context.ExpectedDiagnostics.Add(expected1);
        context.ExpectedDiagnostics.Add(expected2);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidEnum()
    {
        const string testCode = @"
public enum MyEnum
{
    Value1,
    Value2,
    AnotherValue
}
";
        var context = new CSharpAnalyzerTest<DoNotPrefixValuesWithEnumNameAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics as there are no prefix violations
        await context.RunAsync();
    }
}