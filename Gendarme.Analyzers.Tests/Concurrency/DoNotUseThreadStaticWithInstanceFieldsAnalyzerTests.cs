using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotUseThreadStaticWithInstanceFieldsAnalyzer))]
public sealed class DoNotUseThreadStaticWithInstanceFieldsAnalyzerTests
{
    [Fact]
    public async Task TestThreadStaticWithInstanceField()
    {
        const string testCode = @"
using System;
using System.Threading;

public class MyClass
{
    [ThreadStatic]
    public int MyField;
}
";

        var context = new CSharpAnalyzerTest<DoNotUseThreadStaticWithInstanceFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotUseThreadStaticWithInstanceFields)
            .WithSpan(8, 16, 8, 23)
            .WithArguments("MyField");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoThreadStaticOnStaticField()
    {
        const string testCode = @"
using System;
using System.Threading;

public class MyClass
{
    [ThreadStatic]
    public static int MyStaticField;
}
";

        var context = new CSharpAnalyzerTest<DoNotUseThreadStaticWithInstanceFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since the field is static
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoThreadStaticWithNoInstanceField()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public int MyField;
}
";

        var context = new CSharpAnalyzerTest<DoNotUseThreadStaticWithInstanceFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since there's no ThreadStatic attribute
        await context.RunAsync();
    }
}