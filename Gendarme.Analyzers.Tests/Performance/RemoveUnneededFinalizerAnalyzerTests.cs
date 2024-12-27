using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(RemoveUnneededFinalizerAnalyzer))]
public sealed class RemoveUnneededFinalizerAnalyzerTests
{
    [Fact]
    public async Task TestEmptyFinalizer()
    {
        const string testCode = @"
public class MyClass
{
    ~MyClass() { }
}";

        var context = new CSharpAnalyzerTest<RemoveUnneededFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.RemoveUnneededFinalizer)
            .WithSpan(4, 5, 4, 19)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestFinalizerWithNullAssignments()
    {
        const string testCode = @"
public class MyClass
{
    private object _field;

    ~MyClass() 
    {
        _field = null;
    }
}";

        var context = new CSharpAnalyzerTest<RemoveUnneededFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.RemoveUnneededFinalizer)
            .WithSpan(6, 5, 9, 6)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestFinalizerWithNonNullAssignments()
    {
        const string testCode = @"
public class MyClass
{
    private object _field;

    ~MyClass() 
    {
        _field = new object();
    }
}";

        var context = new CSharpAnalyzerTest<RemoveUnneededFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics
        await context.RunAsync();
    }
}