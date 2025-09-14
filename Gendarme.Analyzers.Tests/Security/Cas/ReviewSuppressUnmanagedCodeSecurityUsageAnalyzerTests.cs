using Gendarme.Analyzers.Security.Cas;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(ReviewSuppressUnmanagedCodeSecurityUsageAnalyzer))]
public sealed class ReviewSuppressUnmanagedCodeSecurityUsageAnalyzerTests
{
    [Fact]
    public async Task TestSuppressUnmanagedCodeSecurityUsage()
    {
        const string testCode = @"
using System.Security;

[SuppressUnmanagedCodeSecurity]
public class MyClass { }

public class MyMethodClass
{
    [SuppressUnmanagedCodeSecurity]
    public void MyMethod() { }
}
";

        var context = new CSharpAnalyzerTest<ReviewSuppressUnmanagedCodeSecurityUsageAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expectedClassDiagnostic = new DiagnosticResult(DiagnosticId.ReviewSuppressUnmanagedCodeSecurityUsage, DiagnosticSeverity.Info)
            .WithSpan(5, 14, 5, 21)
            .WithArguments("MyClass");

        var expectedMethodDiagnostic = new DiagnosticResult(DiagnosticId.ReviewSuppressUnmanagedCodeSecurityUsage, DiagnosticSeverity.Info)
            .WithSpan(10, 17, 10, 25)
            .WithArguments("MyMethod");

        context.ExpectedDiagnostics.Add(expectedClassDiagnostic);
        context.ExpectedDiagnostics.Add(expectedMethodDiagnostic);

        await context.RunAsync();
    }
}