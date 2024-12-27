using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(OverrideValueTypeDefaultsAnalyzer))]
public sealed class OverrideValueTypeDefaultsAnalyzerTests
{
    [Fact]
    public async Task TestValueTypeDefaultsOverridden()
    {
        const string testCode = @"
public struct MyStruct
{
    public override bool Equals(object obj) => true;
    public override int GetHashCode() => 0;
}
";

        var context = new CSharpAnalyzerTest<OverrideValueTypeDefaultsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValueTypeDefaultsNotOverridden()
    {
        const string testCode = @"
public struct MyStruct
{
    // Does not override Equals or GetHashCode
}
";

        var context = new CSharpAnalyzerTest<OverrideValueTypeDefaultsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.OverrideValueTypeDefaults)
            .WithSpan(2, 15, 2, 23)
            .WithArguments("MyStruct");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}