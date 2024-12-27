using Gendarme.Analyzers.Security.Cas;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(AddMissingTypeInheritanceDemandAnalyzer))]
public sealed class AddMissingTypeInheritanceDemandAnalyzerTests
{
    [Fact]
    public async Task TestMissingInheritanceDemand()
    {
        const string testCode = @"
using System.Security.Permissions;

[SecurityPermission(SecurityAction.LinkDemand)]
public class MyClass
{
}

public class MyClassWithoutInheritanceDemand : MyClass
{
}
";

        var context = new CSharpAnalyzerTest<AddMissingTypeInheritanceDemandAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AddMissingTypeInheritanceDemand)
            .WithSpan(5, 14, 5, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInheritanceDemandPresent()
    {
        const string testCode = @"
using System.Security.Permissions;

[SecurityPermission(SecurityAction.LinkDemand)]
[SecurityPermission(SecurityAction.InheritanceDemand)]
public class MyClass
{
}
";

        var context = new CSharpAnalyzerTest<AddMissingTypeInheritanceDemandAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestSealedClass()
    {
        const string testCode = @"
using System.Security.Permissions;

[SecurityPermission(SecurityAction.LinkDemand)]
public sealed class MyClass
{
}
";

        var context = new CSharpAnalyzerTest<AddMissingTypeInheritanceDemandAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}