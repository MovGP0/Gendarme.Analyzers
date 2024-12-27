using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(DoNotPrefixEventsWithAfterOrBeforeAnalyzer))]
public sealed class DoNotPrefixEventsWithAfterOrBeforeAnalyzerTests
{
    [Fact]
    public async Task TestEventPrefixViolation()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public event EventHandler AfterSomeEvent;
    public event EventHandler BeforeAnotherEvent;
}
";

        var context = new CSharpAnalyzerTest<DoNotPrefixEventsWithAfterOrBeforeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotPrefixEventsWithAfterOrBefore)
            .WithSpan(6, 21, 6, 43) // Assumed location for AfterSomeEvent
            .WithArguments("AfterSomeEvent");

        var expected2 = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotPrefixEventsWithAfterOrBefore)
            .WithSpan(7, 21, 7, 42) // Assumed location for BeforeAnotherEvent
            .WithArguments("BeforeAnotherEvent");

        context.ExpectedDiagnostics.Add(expected1);
        context.ExpectedDiagnostics.Add(expected2);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEventWithoutDisallowedPrefix()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public event EventHandler SomeEvent;
}
";

        var context = new CSharpAnalyzerTest<DoNotPrefixEventsWithAfterOrBeforeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}