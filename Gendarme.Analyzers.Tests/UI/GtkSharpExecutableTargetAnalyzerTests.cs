using Gendarme.Analyzers.UI;

namespace Gendarme.Analyzers.Tests.UI;

[TestOf(typeof(GtkSharpExecutableTargetAnalyzer))]
public sealed class GtkSharpExecutableTargetAnalyzerTests
{
    [Fact]
    public async Task TestGtkSharpExecutableTarget_NoDiagnosticsForNonConsoleApp()
    {
        const string testCode = @"
using System;

namespace MyNamespace
{
    public class MyClass
    {
        public static void Main()
        {
        }
    }
}";

        var context = new CSharpAnalyzerTest<GtkSharpExecutableTargetAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestGtkSharpExecutableTarget_WithGtkSharpReference()
    {
        const string testCode = @"
using System;

[assembly: System.Reflection.AssemblyMetadata(""IsGtkSharpApplication"", ""True"")]

namespace GtkSharpApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}";

        var context = new CSharpAnalyzerTest<GtkSharpExecutableTargetAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.GtkSharpExecutableTarget)
            .WithSpan(1, 1, 1, 1);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestGtkSharpExecutableTarget_NoDiagnosticsWithoutGtkSharpReference()
    {
        const string testCode = @"
using System;

namespace MyNamespace
{
    public class MyClass
    {
        public static void Main()
        {
        }
    }
}";

        var context = new CSharpAnalyzerTest<GtkSharpExecutableTargetAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}