using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AvoidMultidimensionalIndexerAnalyzer))]
public sealed class AvoidMultidimensionalIndexerAnalyzerTests
{
    [Fact]
    public async Task TestAvoidMultidimensionalIndexer()
    {
        const string testCode = @"
public class MyClass
{
    public int this[int x, int y] { get; set; }
}
";

        var context = new CSharpAnalyzerTest<AvoidMultidimensionalIndexerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidMultidimensionalIndexer)
            .WithSpan(4, 10, 4, 13)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoWarningForSingleDimensionIndexer()
    {
        const string testCode = @"
public class MyClass
{
    public int this[int x] { get; set; }
}
";

        var context = new CSharpAnalyzerTest<AvoidMultidimensionalIndexerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}