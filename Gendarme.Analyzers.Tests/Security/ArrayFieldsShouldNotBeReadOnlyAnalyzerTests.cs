using Gendarme.Analyzers.Security;

namespace Gendarme.Analyzers.Tests.Security;

[TestOf(typeof(ArrayFieldsShouldNotBeReadOnlyAnalyzer))]
public sealed class ArrayFieldsShouldNotBeReadOnlyAnalyzerTests
{
    [Fact]
    public async Task TestPublicReadonlyArrayField()
    {
        const string testCode = @"
public class MyClass
{
    public readonly int[] MyArray = new int[10];
}
";

        var context = new CSharpAnalyzerTest<ArrayFieldsShouldNotBeReadOnlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ArrayFieldsShouldNotBeReadOnly)
            .WithSpan(4, 27, 4, 34)
            .WithArguments("MyArray");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPublicReadonlyNonArrayField()
    {
        const string testCode = @"
public class MyClass
{
    public readonly int MyField = 0;
}
";

        var context = new CSharpAnalyzerTest<ArrayFieldsShouldNotBeReadOnlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestPrivateReadonlyArrayField()
    {
        const string testCode = @"
public class MyClass
{
    private readonly int[] MyArray = new int[10];
}
";

        var context = new CSharpAnalyzerTest<ArrayFieldsShouldNotBeReadOnlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestPublicMutableArrayField()
    {
        const string testCode = @"
public class MyClass
{
    public int[] MyArray = new int[10];
}
";

        var context = new CSharpAnalyzerTest<ArrayFieldsShouldNotBeReadOnlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}