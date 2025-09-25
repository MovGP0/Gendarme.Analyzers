using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(ProtectCallToEventDelegatesAnalyzer))]
public sealed class ProtectCallToEventDelegatesAnalyzerTests
{
    [Fact]
    public async Task DetectsDirectEventInvocation()
    {
        const string testCode = @"using System;

public class MyClass
{
    public event EventHandler MyEvent;

    public void Raise()
    {
        MyEvent(this, EventArgs.Empty);
    }
}";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProtectCallToEventDelegates)
            .WithSpan(9, 9, 9, 16)
            .WithArguments("MyEvent");

        await VerifyAsync(testCode, expected);
    }

    [Fact]
    public async Task DetectsInvokeCallOnEvent()
    {
        const string testCode = @"using System;

public class MyClass
{
    public event EventHandler MyEvent;

    public void Raise()
    {
        MyEvent.Invoke(this, EventArgs.Empty);
    }
}";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProtectCallToEventDelegates)
            .WithSpan(9, 9, 9, 23)
            .WithArguments("MyEvent");

        await VerifyAsync(testCode, expected);
    }

    [Fact]
    public async Task SkipsNullConditionalInvocation()
    {
        const string testCode = @"using System;

public class MyClass
{
    public event EventHandler MyEvent;

    public void Raise()
    {
        MyEvent?.Invoke(this, EventArgs.Empty);
    }
}";

        await VerifyAsync(testCode);
    }

    [Fact]
    public async Task SkipsLocalCopy()
    {
        const string testCode = @"using System;

public class MyClass
{
    public event EventHandler MyEvent;

    public void Raise()
    {
        var handler = MyEvent;
        handler?.Invoke(this, EventArgs.Empty);
    }
}";

        await VerifyAsync(testCode);
    }

    private static Task VerifyAsync(string source, params DiagnosticResult[] expectedDiagnostics)
    {
        var test = new CSharpAnalyzerTest<ProtectCallToEventDelegatesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        return test.RunAsync();
    }
}

