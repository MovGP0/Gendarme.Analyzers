using Gendarme.Analyzers.Portability;

namespace Gendarme.Analyzers.Tests.Portability;

[TestOf(typeof(FeatureRequiresRootPrivilegeOnUnixAnalyzer))]
public sealed class FeatureRequiresRootPrivilegeOnUnixAnalyzerTests
{
    [Fact]
    public async Task TestPingObjectCreation()
    {
        const string testCode = @"
using System.Net.NetworkInformation;

public class MyClass
{
    public MyClass()
    {
        Ping ping = new Ping();
    }
}";

        var context = new CSharpAnalyzerTest<FeatureRequiresRootPrivilegeOnUnixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.FeatureRequiresRootPrivilegeOnUnix)
            .WithSpan(8, 21, 8, 31);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPriorityClassAssignment()
    {
        const string testCode = @"
using System.Diagnostics;

public class MyClass
{
    public void SetProcessPriority(Process process)
    {
        process.PriorityClass = ProcessPriorityClass.High; // This should trigger a warning
    }
}";

        var context = new CSharpAnalyzerTest<FeatureRequiresRootPrivilegeOnUnixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.FeatureRequiresRootPrivilegeOnUnix)
            .WithSpan(8, 9, 8, 58);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNormalPriorityClassAssignment()
    {
        const string testCode = @"
using System.Diagnostics;

public class MyClass
{
    public void SetProcessPriority(Process process)
    {
        process.PriorityClass = ProcessPriorityClass.Normal; // This should NOT trigger a warning
    }
}";

        var context = new CSharpAnalyzerTest<FeatureRequiresRootPrivilegeOnUnixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}