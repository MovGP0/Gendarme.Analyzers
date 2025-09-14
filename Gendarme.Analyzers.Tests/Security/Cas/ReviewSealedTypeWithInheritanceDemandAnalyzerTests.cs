using Gendarme.Analyzers.Security.Cas;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(ReviewSealedTypeWithInheritanceDemandAnalyzer))]
public sealed class ReviewSealedTypeWithInheritanceDemandAnalyzerTests
{
    [Fact]
    public async Task TestSealedClassWithInheritanceDemand()
    {
        const string testCode = @"
using System.Security.Permissions;

[SecurityPermission(SecurityAction.InheritanceDemand)]
public sealed class MySealedClass { }

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<ReviewSealedTypeWithInheritanceDemandAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.ReviewSealedTypeWithInheritanceDemand, DiagnosticSeverity.Info)
            .WithSpan(5, 21, 5, 34)
            .WithArguments("MySealedClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonSealedClassWithInheritanceDemand()
    {
        const string testCode = @"
using System.Security.Permissions;

[SecurityPermission(SecurityAction.InheritanceDemand)]
public class MyNonSealedClass { }

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<ReviewSealedTypeWithInheritanceDemandAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for non-sealed class.
        await context.RunAsync();
    }

    [Fact]
    public async Task TestSealedClassWithoutInheritanceDemand()
    {
        const string testCode = @"
public sealed class MySealedClass { }

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<ReviewSealedTypeWithInheritanceDemandAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for sealed class without inheritance demand.
        await context.RunAsync();
    }
}