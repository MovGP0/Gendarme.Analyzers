using Gendarme.Analyzers.Security;

namespace Gendarme.Analyzers.Tests.Security;

[TestOf(typeof(DoNotShortCircuitCertificateCheckAnalyzer))]
public sealed class DoNotShortCircuitCertificateCheckAnalyzerTests
{
    [Fact]
    public async Task TestShortCircuitingCertificateCheck()
    {
        const string testCode = @"
using System;
public class CertificateValidator
{
    public bool CheckValidationResult()
    {
        return true; // This should trigger a diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotShortCircuitCertificateCheckAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotShortCircuitCertificateCheck)
            .WithSpan(7, 9, 7, 21)
            .WithArguments("CheckValidationResult");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestNotShortCircuitingCertificateCheck()
    {
        const string testCode = @"
using System;
public class CertificateValidator
{
    public bool CheckValidationResult()
    {
        // Do some validation logic here
        return false; // This should not trigger a diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotShortCircuitCertificateCheckAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestCertificateValidationCallback()
    {
        const string testCode = @"
using System;
public class CertificateValidator
{
    public bool CertificateValidationCallback()
    {
        return true; // This should trigger a diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotShortCircuitCertificateCheckAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotShortCircuitCertificateCheck)
            .WithSpan(7, 9, 7, 21)
            .WithArguments("CertificateValidationCallback");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}