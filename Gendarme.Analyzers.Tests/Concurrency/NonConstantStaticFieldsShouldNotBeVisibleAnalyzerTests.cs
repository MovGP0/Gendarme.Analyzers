using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(NonConstantStaticFieldsShouldNotBeVisibleAnalyzer))]
public sealed class NonConstantStaticFieldsShouldNotBeVisibleAnalyzerTests
{
    [Fact]
    public async Task TestNonConstantStaticFieldIsPublic()
    {
        const string testCode = @"
public class MyClass
{
    public static int NonConstantField; // This should trigger a diagnostic
}
";

        var context = new CSharpAnalyzerTest<NonConstantStaticFieldsShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.NonConstantStaticFieldsShouldNotBeVisible)
            .WithSpan(4, 23, 4, 39)
            .WithArguments("NonConstantField");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestConstStaticFieldIsPublicDoesNotTrigger()
    {
        const string testCode = @"
public class MyClass
{
    public const int ConstantField = 42; // This should not trigger a diagnostic
}
";

        var context = new CSharpAnalyzerTest<NonConstantStaticFieldsShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestReadOnlyStaticFieldIsPublicDoesNotTrigger()
    {
        const string testCode = @"
public class MyClass
{
    public static readonly int ReadOnlyField = 42; // This should not trigger a diagnostic
}
";

        var context = new CSharpAnalyzerTest<NonConstantStaticFieldsShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPrivateStaticFieldDoesNotTrigger()
    {
        const string testCode = @"
public class MyClass
{
    private static int NonConstantPrivateField; // This should not trigger a diagnostic
}
";

        var context = new CSharpAnalyzerTest<NonConstantStaticFieldsShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}