using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(DeclareEventHandlersCorrectlyAnalyzer))]
public sealed class DeclareEventHandlersCorrectlyAnalyzerTests
{
    [Fact]
    public async Task TestEventHandlerWithIncorrectParameters()
    {
        const string testCode = @"
using System;

public delegate void MyCustomEventHandler(string arg1, int arg2);

public class MyClass
{
    public event MyCustomEventHandler MyEvent;
}
";

        var context = new CSharpAnalyzerTest<DeclareEventHandlersCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DeclareEventHandlersCorrectly)
            .WithSpan(7, 14, 7, 20)
            .WithArguments("MyEvent");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEventHandlerWithIncorrectReturnType()
    {
        const string testCode = @"
using System;

public delegate int MyCustomEventHandler(object sender, EventArgs e);

public class MyClass
{
    public event MyCustomEventHandler MyEvent;
}
";

        var context = new CSharpAnalyzerTest<DeclareEventHandlersCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DeclareEventHandlersCorrectly)
            .WithSpan(7, 14, 7, 20)
            .WithArguments("MyEvent");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEventHandlerWithCorrectSignature()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public event EventHandler MyEvent;
}
";

        var context = new CSharpAnalyzerTest<DeclareEventHandlersCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // Expect no diagnostics
    }
}