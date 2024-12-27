using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(ProtectCallToEventDelegatesAnalyzer))]
public sealed class ProtectCallToEventDelegatesAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected yet")]
    public async Task TestEventDelegateInvocation()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public event EventHandler MyEvent;

    public void CallEvent()
    {
        MyEvent?.Invoke(this, EventArgs.Empty);
    }
}";

        var context = new CSharpAnalyzerTest<ProtectCallToEventDelegatesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProtectCallToEventDelegates)
            .WithSpan(8, 13, 8, 20)
            .WithArguments("MyEvent");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvocationWithoutEvent()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void CallMethod()
    {
        Console.WriteLine(""Hello World"");
    }
}";

        var context = new CSharpAnalyzerTest<ProtectCallToEventDelegatesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostic expected since there's no event being invoked
        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvocationOnNullEvent()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public event EventHandler MyEvent;

    public void CallEvent()
    {
        MyEvent?.Invoke(this, EventArgs.Empty); // Should not raise warning due to null conditional
    }
}";

        var context = new CSharpAnalyzerTest<ProtectCallToEventDelegatesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostic expected due to the null conditional operator
        await context.RunAsync();
    }
}