using Gendarme.Analyzers.Design.Generic;

namespace Gendarme.Analyzers.Tests.Design.Generic;

[TestOf(typeof(PreferGenericsOverRefObjectAnalyzer))]
public sealed class PreferGenericsOverRefObjectAnalyzerTests
{
    [Fact]
    public async Task TestPreferGenericsOverRefObject()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod(ref object obj) { }
}
";

        var context = new CSharpAnalyzerTest<PreferGenericsOverRefObjectAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferGenericsOverRefObject)
            .WithSpan(4, 26, 4, 40);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoDiagnosticsForGenerics()
    {
        const string testCode = @"
using System.Collections.Generic;

public class MyClass
{
    public void MyMethod(List<object> obj) { }
}
";

        var context = new CSharpAnalyzerTest<PreferGenericsOverRefObjectAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics

        await context.RunAsync();
    }
}