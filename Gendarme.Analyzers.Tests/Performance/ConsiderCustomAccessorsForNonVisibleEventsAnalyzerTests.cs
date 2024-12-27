using Gendarme.Analyzers.Performance;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(ConsiderCustomAccessorsForNonVisibleEventsAnalyzer))]
public sealed class ConsiderCustomAccessorsForNonVisibleEventsAnalyzerTests
{
    [Fact]
    public async Task TestNonVisibleEventWithCustomAccessors()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private event EventHandler MyEvent;
}
";

        var context = new CSharpAnalyzerTest<ConsiderCustomAccessorsForNonVisibleEventsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };
        
        var expected = new DiagnosticResult(DiagnosticId.ConsiderCustomAccessorsForNonVisibleEvents, DiagnosticSeverity.Info)
            .WithSpan(6, 32, 6, 39)
            .WithArguments("MyEvent");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestPublicEventNoDiagnostic()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public event EventHandler MyPublicEvent;
}
";

        var context = new CSharpAnalyzerTest<ConsiderCustomAccessorsForNonVisibleEventsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestProtectedEventNoDiagnostic()
    {
        const string testCode = @"
using System;

public class MyBaseClass
{
    protected event EventHandler MyProtectedEvent;
}
";

        var context = new CSharpAnalyzerTest<ConsiderCustomAccessorsForNonVisibleEventsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}