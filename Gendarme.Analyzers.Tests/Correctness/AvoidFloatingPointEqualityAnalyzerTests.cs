using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(AvoidFloatingPointEqualityAnalyzer))]
public sealed class AvoidFloatingPointEqualityAnalyzerTests
{
    [Fact]
    public async Task TestFloatingPointEquality()
    {
        const string testCode = @"
        public class MyClass
        {
            public void Compare()
            {
                float a = 0.1f;
                float b = 0.1f;
                if (a == b) { } // Should trigger warning
            }
        }
        ";

        var context = new CSharpAnalyzerTest<AvoidFloatingPointEqualityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidFloatingPointEquality)
            .WithSpan(8, 21, 8, 27);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestFloatingPointInequality()
    {
        const string testCode = @"
        public class MyClass
        {
            public void Compare()
            {
                double x = 0.2;
                double y = 0.2;
                if (x != y) { } // Should trigger warning
            }
        }
        ";

        var context = new CSharpAnalyzerTest<AvoidFloatingPointEqualityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidFloatingPointEquality)
            .WithSpan(8, 21, 8, 27);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}