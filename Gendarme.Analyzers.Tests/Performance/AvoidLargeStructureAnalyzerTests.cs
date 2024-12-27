using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidLargeStructureAnalyzer))]
public sealed class AvoidLargeStructureAnalyzerTests
{
    [Fact]
    public async Task TestLargeStructWarning()
    {
        const string testCode = @"
public struct LargeStruct
{
    public int Field1;
    public double Field2;
    public decimal Field3;
    public long Field4;
}
";

        var context = new CSharpAnalyzerTest<AvoidLargeStructureAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidLargeStructure)
            .WithSpan(2, 15, 2, 26)
            .WithArguments("LargeStruct", 16); // Assuming the size exceeds 16 bytes

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestSmallStructNoWarning()
    {
        const string testCode = @"
public struct SmallStruct
{
    public byte Field1;
    public int Field2;
}
";

        var context = new CSharpAnalyzerTest<AvoidLargeStructureAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics as the size is within limit
        await context.RunAsync();
    }
}