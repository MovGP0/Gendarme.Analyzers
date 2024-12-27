using Gendarme.Analyzers.Design.Linq;

namespace Gendarme.Analyzers.Tests.Design.Linq;

[TestOf(typeof(AvoidExtensionMethodOnSystemObjectAnalyzer))]
public sealed class AvoidExtensionMethodOnSystemObjectAnalyzerTests
{
    [Fact]
    public async Task TestExtensionMethodOnSystemObject()
    {
        const string testCode = @"
using System;

public static class MyExtensions
{
    public static void MyMethod(this object obj) { }
}
";

        var context = new CSharpAnalyzerTest<AvoidExtensionMethodOnSystemObjectAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidExtensionMethodOnSystemObject)
            .WithSpan(6, 33, 6, 48)
            .WithArguments("MyMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidExtensionMethod()
    {
        const string testCode = @"
using System;

public static class MyExtensions
{
    public static void MyMethod(this string str) { }
}
";

        var context = new CSharpAnalyzerTest<AvoidExtensionMethodOnSystemObjectAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}