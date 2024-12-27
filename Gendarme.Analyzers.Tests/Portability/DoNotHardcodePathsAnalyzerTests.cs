using Gendarme.Analyzers.Portability;

namespace Gendarme.Analyzers.Tests.Portability;

[TestOf(typeof(DoNotHardcodePathsAnalyzer))]
public sealed class DoNotHardcodePathsAnalyzerTests
{
    [Fact]
    public async Task TestHardcodedPathWarning()
    {
        const string testCode = @"
class MyClass
{
    void MyMethod()
    {
        string path = @""C:\Program Files\MyApp"";
    }
}";

        var context = new CSharpAnalyzerTest<DoNotHardcodePathsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotHardcodePaths)
            .WithSpan(6, 23, 6, 48);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoHardcodedPath_NoDiagnostics()
    {
        const string testCode = @"
class MyClass
{
    void MyMethod()
    {
        string message = ""Hello, World!"";
    }
}";

        var context = new CSharpAnalyzerTest<DoNotHardcodePathsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}