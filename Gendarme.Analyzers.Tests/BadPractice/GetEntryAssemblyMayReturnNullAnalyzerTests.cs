using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(GetEntryAssemblyMayReturnNullAnalyzer))]
public sealed class GetEntryAssemblyMayReturnNullAnalyzerTests
{
    [Fact]
    public async Task TestGetEntryAssemblyInLibrary()
    {
        const string testCode = @"
using System.Reflection;

public class MyLibrary
{
    public void SomeMethod()
    {
        var assembly = Assembly.GetEntryAssembly();
    }
}
";

        var context = new CSharpAnalyzerTest<GetEntryAssemblyMayReturnNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.GetEntryAssemblyMayReturnNull)
            .WithSpan(8, 24, 8, 49);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}