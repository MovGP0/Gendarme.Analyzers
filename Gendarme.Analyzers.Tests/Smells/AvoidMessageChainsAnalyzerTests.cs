using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidMessageChainsAnalyzer))]
public sealed class AvoidMessageChainsAnalyzerTests
{
    [Fact]
    public async Task TestTooManyMemberAccesses()
    {
        // This code defines A, B, and C so that
        // a.b.c.d compiles and triggers the diagnostic.
        const string testCode = @"
public class MyClass
{
    private readonly A a = new A();

    public void MyMethod()
    {
        var result = a.b.c.d; // This should trigger a diagnostic
    }
}

public class A
{
    public B b { get; } = new B();
}

public class B
{
    public C c { get; } = new C();
}

public class C
{
    public string d => ""Hello"";
}
";

        var context = new CSharpAnalyzerTest<AvoidMessageChainsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // Adjust the .WithSpan(...) to match the line/column
        // where the analyzer should report a diagnostic.
        // Here, 'var result = a.b.c.d' is on line 8 in the snippet above.
        // The column span can be refined if needed, but 22..32 should capture "a.b.c.d".
        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidMessageChains)
            .WithSpan(8, 22, 8, 32)
            .WithArguments("MyMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidMemberAccesses()
    {
        // A simpler code snippet with only a.b should *not* trigger the diagnostic.
        const string testCode = @"
public class MyClass
{
    private readonly A a = new A();

    public void MyMethod()
    {
        var result = a.b; // This should NOT trigger a diagnostic
    }
}

public class A
{
    public A b => new A();
}
";

        var context = new CSharpAnalyzerTest<AvoidMessageChainsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected here
        await context.RunAsync();
    }
}