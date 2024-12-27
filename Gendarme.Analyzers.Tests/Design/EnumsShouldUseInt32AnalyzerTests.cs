using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(EnumsShouldUseInt32Analyzer))]
public sealed class EnumsShouldUseInt32AnalyzerTests
{
    [Fact]
    public async Task TestEnumWithInt32UnderlyingType()
    {
        const string testCode = @"
public enum MyEnum : System.Int32
{
    Value1,
    Value2
}";

        var context = new CSharpAnalyzerTest<EnumsShouldUseInt32Analyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEnumWithByteUnderlyingType()
    {
        const string testCode = @"
public enum MyEnum : System.Byte
{
    Value1,
    Value2
}";

        var context = new CSharpAnalyzerTest<EnumsShouldUseInt32Analyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.EnumsShouldUseInt32)
            .WithSpan(1, 15, 1, 22)
            .WithArguments("MyEnum");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEnumWithLongUnderlyingType()
    {
        const string testCode = @"
public enum MyEnum : System.Int64
{
    Value1,
    Value2
}";

        var context = new CSharpAnalyzerTest<EnumsShouldUseInt32Analyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.EnumsShouldUseInt32)
            .WithSpan(1, 15, 1, 22)
            .WithArguments("MyEnum");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}