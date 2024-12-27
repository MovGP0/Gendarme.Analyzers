using Gendarme.Analyzers.Security.Cas;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(DoNotReduceTypeSecurityOnMethodsAnalyzer))]
public sealed class DoNotReduceTypeSecurityOnMethodsAnalyzerTests
{
    [Fact]
    public async Task TestMethodSecurityReductions()
    {
        const string testCode = @"
using System.Security.Permissions;

[SecurityPermission(SecurityAction.Deny)]
public class Parent
{
    [SecurityPermission(SecurityAction.Assert)]
    public virtual void TestMethod() { }
}

public class Child : Parent
{
    // This method should cause a diagnostic since it reduces security
    public override void TestMethod() { }
}
";

        var context = new CSharpAnalyzerTest<DoNotReduceTypeSecurityOnMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotReduceTypeSecurityOnMethods)
            .WithSpan(8, 25, 8, 35)
            .WithArguments("TestMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}