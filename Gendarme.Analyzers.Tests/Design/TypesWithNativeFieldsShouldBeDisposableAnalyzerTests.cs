using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(TypesWithNativeFieldsShouldBeDisposableAnalyzer))]
public sealed class TypesWithNativeFieldsShouldBeDisposableAnalyzerTests
{
    [Fact]
    public async Task TestNonDisposableTypeWithNativeField()
    {
        const string testCode = @"
public class MyClass 
{
    private IntPtr handle; // Native field
}
";

        var context = new CSharpAnalyzerTest<TypesWithNativeFieldsShouldBeDisposableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.TypesWithNativeFieldsShouldBeDisposable)
            .WithSpan(4, 10, 4, 21)
            .WithArguments("MyClass", "handle");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestDisposableTypeWithNativeField()
    {
        const string testCode = @"
using System;
public class MyDisposableClass : IDisposable
{
    private IntPtr handle; // Native field

    public void Dispose() { }
}
";

        var context = new CSharpAnalyzerTest<TypesWithNativeFieldsShouldBeDisposableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassWithoutNativeFields()
    {
        const string testCode = @"
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<TypesWithNativeFieldsShouldBeDisposableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}