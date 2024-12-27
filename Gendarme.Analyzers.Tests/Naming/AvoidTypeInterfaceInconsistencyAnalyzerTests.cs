using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(AvoidTypeInterfaceInconsistencyAnalyzer))]
public sealed class AvoidTypeInterfaceInconsistencyAnalyzerTests
{
    [Fact]
    public async Task TestInterfaceClassInconsistency()
    {
        const string testCode = @"
            public interface IMyInterface { }
            public class MyClass : IMyInterface { }

            public class MyInvalidClass { }
            public class IMyInvalidClass : MyInvalidClass { }
        ";

        var context = new CSharpAnalyzerTest<AvoidTypeInterfaceInconsistencyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidTypeInterfaceInconsistency)
            .WithSpan(8, 22, 8, 40) // Assuming location this is reported
            .WithArguments("MyInvalidClass", "IMyInvalidClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidInterfaceAndClass()
    {
        const string testCode = @"
            public interface IValidInterface { }
            public class ValidClass : IValidInterface { }
        ";

        var context = new CSharpAnalyzerTest<AvoidTypeInterfaceInconsistencyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}