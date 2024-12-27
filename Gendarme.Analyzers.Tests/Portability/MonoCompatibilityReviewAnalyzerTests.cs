using Gendarme.Analyzers.Portability;

namespace Gendarme.Analyzers.Tests.Portability;

[TestOf(typeof(MonoCompatibilityReviewAnalyzer))]
public sealed class MonoCompatibilityReviewAnalyzerTests
{
    [Fact]
    public async Task TestIncompatibleMethodInvocation()
    {
        const string testCode = @"
using System.Drawing;

public class TestClass
{
    public void TestMethod()
    {
        var bitmap = new Bitmap(1, 1);
        bitmap.SetResolution(100, 100); // Incompatible method
    }
}
";

        var context = new CSharpAnalyzerTest<MonoCompatibilityReviewAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80Windows,
            TestCode = testCode
        };

        // Make sure the .WithSpan(...) matches the line of the incompatible call.
        // In the snippet above, `bitmap.SetResolution(100, 100)` is on line 8.
        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MonoCompatibilityReview)
            .WithSpan(9, 9, 9, 39);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCompatibleMethodInvocation()
    {
        const string testCode = @"
using System.Drawing;

public class TestClass
{
    public void TestMethod()
    {
        using var bmp = new Bitmap(100, 100);
        using var graphics = Graphics.FromImage(bmp);
        // The DrawRectangle call below is typically allowed in mono-compatible scenarios.
        graphics.DrawRectangle(new Pen(Color.Black), 0, 0, 100, 100); // Compatible method
    }
}
";

        var context = new CSharpAnalyzerTest<MonoCompatibilityReviewAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80Windows,
            TestCode = testCode
        };

        // No diagnostic should be reported here.
        await context.RunAsync();
    }
}
