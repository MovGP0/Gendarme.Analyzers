using Gendarme.Analyzers.Security.Cas;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(DoNotExposeFieldsInSecuredTypeAnalyzer))]
public sealed class DoNotExposeFieldsInSecuredTypeAnalyzerTests
{
    [Fact]
    public async Task TestExposedPublicFieldsInSecuredType()
    {
        const string testCode = @"
using System.Security.Permissions;

[SecurityPermission(SecurityAction.Demand)]
public class SecuredClass
{
    public int ExposedField;
}

public class RegularClass
{
    public int NotSecuredField;
}
";

        var context = new CSharpAnalyzerTest<DoNotExposeFieldsInSecuredTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotExposeFieldsInSecuredType)
            .WithSpan(5, 14, 5, 26)
            .WithArguments("SecuredClass", "ExposedField");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoDiagnosticsForNonSecuredType()
    {
        const string testCode = @"
public class RegularClass
{
    public int NotSecuredField;
}
";

        var context = new CSharpAnalyzerTest<DoNotExposeFieldsInSecuredTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}