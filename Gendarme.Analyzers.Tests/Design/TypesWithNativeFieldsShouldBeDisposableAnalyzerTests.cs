using Gendarme.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(TypesWithNativeFieldsShouldBeDisposableAnalyzer))]
public sealed class TypesWithNativeFieldsShouldBeDisposableAnalyzerTests
{
    [Fact]
    public async Task TestNonDisposableTypeWithNativeField()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass
{
    private System.IntPtr handle;  // Use fully qualified name
}";

        var context = new CSharpAnalyzerTest<TypesWithNativeFieldsShouldBeDisposableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode,
            TestState = 
            {
                AdditionalReferences = 
                {
                    MetadataReference.CreateFromFile(typeof(System.IntPtr).Assembly.Location)
                }
            }
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.TypesWithNativeFieldsShouldBeDisposable)
            .WithSpan(17, 13, 17, 19)  // points to "handle" in the field declaration
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