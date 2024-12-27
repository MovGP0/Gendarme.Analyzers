using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidDeepInheritanceTreeAnalyzer))]
public sealed class AvoidDeepInheritanceTreeAnalyzerTests
{
    [Fact]
    public async Task TestDeepInheritanceTree()
    {
        const string testCode = @"
        public class A { }
        public class B : A { }
        public class C : B { }
        public class D : C { }
        public class E : D { } // This should trigger the diagnostic

        public class F : E { } // This is the 5th level in the inheritance tree
        ";

        var context = new CSharpAnalyzerTest<AvoidDeepInheritanceTreeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidDeepInheritanceTree)
            .WithSpan(8, 22, 8, 23)
            .WithArguments("F", 5);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidInheritanceTree()
    {
        const string testCode = @"
        public class A { }
        public class B : A { }
        public class C : B { }
        public class D : C { }
        ";

        var context = new CSharpAnalyzerTest<AvoidDeepInheritanceTreeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected in this case
        await context.RunAsync();
    }
}