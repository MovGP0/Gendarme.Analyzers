using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(ObsoleteMessagesShouldNotBeEmptyAnalyzer))]
public sealed class ObsoleteMessagesShouldNotBeEmptyAnalyzerTests
{
    [Fact]
    public async Task TestObsoleteAttributeWithEmptyMessage()
    {
        const string testCode = @"
using System;

[Obsolete]
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<ObsoleteMessagesShouldNotBeEmptyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ObsoleteMessagesShouldNotBeEmpty)
            .WithSpan(4, 2, 4, 10)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }

    [Fact]
    public async Task TestObsoleteAttributeWithValidMessage()
    {
        const string testCode = @"
using System;

[Obsolete(""Use NewClass instead"")]
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<ObsoleteMessagesShouldNotBeEmptyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestObsoleteAttributeWithEmptyMessageArgument()
    {
        const string testCode = @"
using System;

[Obsolete("""")]
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<ObsoleteMessagesShouldNotBeEmptyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ObsoleteMessagesShouldNotBeEmpty)
            .WithSpan(4, 2, 4, 14)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }
}