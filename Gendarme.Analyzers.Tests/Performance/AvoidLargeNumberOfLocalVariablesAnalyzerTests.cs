using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidLargeNumberOfLocalVariablesAnalyzer))]
public sealed class AvoidLargeNumberOfLocalVariablesAnalyzerTests
{
    [Fact]
    public async Task TestTooManyLocalVariables()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod()
            {
                int a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, 
                    a11, a12, a13, a14, a15, a16, 
                    a17, a18, a19, a20, a21, 
                    a22, a23, a24, a25, a26, 
                    a27, a28, a29, a30, a31, 
                    a32, a33, a34, a35, a36, 
                    a37, a38, a39, a40, a41, 
                    a42, a43, a44, a45, a46, 
                    a47, a48, a49, a50, a51, 
                    a52, a53, a54, a55, a56, 
                    a57, a58, a59, a60, a61, 
                    a62, a63, a64, a65; // 65 variables
            }
        }";

        var context = new CSharpAnalyzerTest<AvoidLargeNumberOfLocalVariablesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidLargeNumberOfLocalVariables)
            .WithSpan(4, 25, 4, 33)
            .WithArguments("MyMethod", 65, 64);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidLocalVariablesCount()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod()
            {
                int a1, a2, a3; // 3 variables
            }
        }";

        var context = new CSharpAnalyzerTest<AvoidLargeNumberOfLocalVariablesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since the count is within limits
        await context.RunAsync();
    }
}