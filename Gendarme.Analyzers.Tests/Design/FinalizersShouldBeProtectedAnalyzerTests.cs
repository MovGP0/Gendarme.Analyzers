using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(FinalizersShouldBeProtectedAnalyzer))]
public sealed class FinalizersShouldBeProtectedAnalyzerTests
{
    [Fact]
    public async Task TestFinalizerImplementation()
    {
        const string testCode = @"
using System;

public class MyClass : IDisposable
{
    ~MyClass() { }  // Implicit finalizer
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}";

        var context = new CSharpAnalyzerTest<FinalizersShouldBeProtectedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostic expected since C# finalizers are implicitly protected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestPrivateFinalize()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private void Finalize() { }  // Not a real finalizer, but a private method named Finalize
}";

        var context = new CSharpAnalyzerTest<FinalizersShouldBeProtectedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostic expected because this isn't actually a finalizer (MethodKind.Destructor)
        await context.RunAsync();
    }
}