using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidSwitchStatementsAnalyzer))]
public sealed class AvoidSwitchStatementsAnalyzerTests
{
    [Fact]
    public async Task TestAvoidSwitchStatement()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod(int option)
            {
                switch (option)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                }
            }
        }";

        var context = new CSharpAnalyzerTest<AvoidSwitchStatementsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidSwitchStatements)
            .WithSpan(6, 17, 6, 23)
            .WithArguments("MyMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoSwitchStatement_NoDiagnostic()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod(int option)
            {
                if (option == 0)
                {
                    // do something
                }
            }
        }";

        var context = new CSharpAnalyzerTest<AvoidSwitchStatementsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}