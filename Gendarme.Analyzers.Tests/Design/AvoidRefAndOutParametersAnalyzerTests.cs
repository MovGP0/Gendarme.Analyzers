using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AvoidRefAndOutParametersAnalyzer))]
public sealed class AvoidRefAndOutParametersAnalyzerTests
{
    [Fact]
    public async Task TestMethodWithRefParameter()
    {
        const string testCode = @"
        public class MyClass 
        {
            public void MethodWithRef(ref int number) { }
        }";

        var context = new CSharpAnalyzerTest<AvoidRefAndOutParametersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidRefAndOutParameters)
            .WithSpan(4, 21, 4, 31)
            .WithArguments("MethodWithRef", "number");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestMethodWithOutParameter()
    {
        const string testCode = @"
        public class MyClass 
        {
            public void MethodWithOut(out int number) { number = 0; }
        }";

        var context = new CSharpAnalyzerTest<AvoidRefAndOutParametersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidRefAndOutParameters)
            .WithSpan(4, 21, 4, 30)
            .WithArguments("MethodWithOut", "number");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodWithTryPatternOutParameter()
    {
        const string testCode = @"
        public class MyClass 
        {
            public bool TryGetSomething(out int result) { result = 0; return true; }
        }";

        var context = new CSharpAnalyzerTest<AvoidRefAndOutParametersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since this follows the Try pattern.
        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodWithMixedParameters()
    {
        const string testCode = @"
        public class MyClass 
        {
            public bool TryParse(string input, out int result, ref int errorCode) 
            { 
                result = 0; 
                errorCode = 0; 
                return true; 
            }
        }";

        var context = new CSharpAnalyzerTest<AvoidRefAndOutParametersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidRefAndOutParameters)
            .WithSpan(6, 38, 6, 47)
            .WithArguments("TryParse", "errorCode");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}