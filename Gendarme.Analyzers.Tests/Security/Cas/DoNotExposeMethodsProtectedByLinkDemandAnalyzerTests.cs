using Gendarme.Analyzers.Security.Cas;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(DoNotExposeMethodsProtectedByLinkDemandAnalyzer))]
public sealed class DoNotExposeMethodsProtectedByLinkDemandAnalyzerTests
{
    [Fact]
    public async Task TestMethodProtectedByLinkDemand_InvokedByWeakerCaller_MethodIsReported()
    {
        const string testCode = @"
using System;
using System.Security.Permissions;

public class Caller
{
    public void Call()
    {
        TargetMethod();
    }

    [SecurityPermission(SecurityAction.LinkDemand)]
    public void TargetMethod() { }
}
";

        var context = new CSharpAnalyzerTest<DoNotExposeMethodsProtectedByLinkDemandAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotExposeMethodsProtectedByLinkDemand)
            .WithSpan(9, 9, 9, 23)
            .WithArguments("Call");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodProtectedByLinkDemand_InvokedByStrongerCaller_NoDiagnostics()
    {
        const string testCode = @"
using System;
using System.Security.Permissions;

public class StrongCaller
{
    [SecurityPermission(SecurityAction.LinkDemand)]
    public void Call()
    {
        TargetMethod();
    }

    [SecurityPermission(SecurityAction.LinkDemand)]
    public void TargetMethod() { }
}
";

        var context = new CSharpAnalyzerTest<DoNotExposeMethodsProtectedByLinkDemandAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodNotProtectedByLinkDemand_NoDiagnostics()
    {
        const string testCode = @"
using System;

public class Caller
{
    public void Call()
    {
        TargetMethod();
    }

    public void TargetMethod() { }
}
";

        var context = new CSharpAnalyzerTest<DoNotExposeMethodsProtectedByLinkDemandAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}