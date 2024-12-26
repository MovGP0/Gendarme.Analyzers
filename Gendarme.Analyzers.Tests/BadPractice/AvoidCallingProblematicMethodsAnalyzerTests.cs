using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(AvoidCallingProblematicMethodsAnalyzer))]
public sealed class AvoidCallingProblematicMethodsAnalyzerTests
{
    [Fact]
    public async Task TestGCCollectInvocation()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void CallGCCollect()
    {
        GC.Collect();
    }
}";

        var context = new CSharpAnalyzerTest<AvoidCallingProblematicMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidCallingProblematicMethods)
            .WithSpan(8, 9, 8, 21)
            .WithArguments("Collect");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestThreadSuspendInvocation()
    {
        const string testCode = @"
using System.Threading;

public class MyClass
{
    public void SuspendThread(Thread thread)
    {
        thread.Suspend();
    }
}";

        var context = new CSharpAnalyzerTest<AvoidCallingProblematicMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidCallingProblematicMethods)
            .WithSpan(8, 9, 8, 25)
            .WithArguments("Suspend");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvokeMemberWithBindingFlagsNonPublic()
    {
        const string testCode = @"
using System;
using System.Reflection;

public class MyClass
{
    public void CallInvokeMember()
    {
        typeof(MyClass).InvokeMember(""MethodName"", BindingFlags.NonPublic, null, null, null);
    }
}";

        var context = new CSharpAnalyzerTest<AvoidCallingProblematicMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidCallingProblematicMethods)
            .WithSpan(9, 9, 9, 93)
            .WithArguments("InvokeMember");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAssemblyLoadFromInvocation()
    {
        const string testCode = @"
using System.Reflection;

public class MyClass
{
    public void LoadAssembly()
    {
        Assembly.LoadFrom(""path/to/assembly"");
    }
}";

        var context = new CSharpAnalyzerTest<AvoidCallingProblematicMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidCallingProblematicMethods)
            .WithSpan(8, 9, 8, 46)
            .WithArguments("LoadFrom");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}