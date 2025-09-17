using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(TypesWithDisposableFieldsShouldBeDisposableAnalyzer))]
public sealed class TypesWithDisposableFieldsShouldBeDisposableAnalyzerTests
{
    [Fact]
    public async Task TestDisposableFieldWithoutIDisposable()
    {
        const string testCode = @"
using System;

public class MyDisposable : IDisposable
{
    public void Dispose() { }
}

public class MyClass : IDisposable
{
    private MyDisposable disposableField;

    public void Dispose() { }
}

public class MyClassWithoutDisposable
{
    private MyDisposable disposableField; // Has disposable field but does not implement IDisposable
}
";

        var context = new CSharpAnalyzerTest<TypesWithDisposableFieldsShouldBeDisposableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.TypesWithDisposableFieldsShouldBeDisposable)
            .WithSpan(18, 26, 18, 41)
            .WithArguments("MyClassWithoutDisposable", "disposableField");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestDisposableFieldWithIDisposable()
    {
        const string testCode = @"
using System;

public class MyDisposable : IDisposable
{
    public void Dispose() { }
}

public class MyClass : IDisposable
{
    private MyDisposable disposableField;

    public void Dispose() { }
}
";

        var context = new CSharpAnalyzerTest<TypesWithDisposableFieldsShouldBeDisposableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected here
        await context.RunAsync();
    }
}