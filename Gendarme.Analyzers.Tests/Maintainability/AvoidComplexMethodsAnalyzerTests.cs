using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidComplexMethodsAnalyzer))]
public sealed class AvoidComplexMethodsAnalyzerTests
{
    [Fact]
    public async Task TestMethodComplexityExceedsThreshold()
    {
        const string testCode = @"
public class MyClass
{
    public void ComplexMethod()
    {
        if (true)
        {
            if (false) { }
            for (int i = 0; i < 10; i++) { }
        }
        else
        {
            while (true) { }
        }
    }
}";

        var context = new CSharpAnalyzerTest<AvoidComplexMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidComplexMethods)
            .WithSpan(6, 16, 6, 31) // Adjust the span based on the actual output
            .WithArguments("ComplexMethod", 5); // Replace with actual complexity calculation
        
        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodComplexityBelowThreshold()
    {
        const string testCode = @"
public class MyClass
{
    public void SimpleMethod()
    {
        if (true) { }
    }
}";

        var context = new CSharpAnalyzerTest<AvoidComplexMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestConstructorComplexityExceedsThreshold()
    {
        const string testCode = @"
public class MyClass
{
    public MyClass()
    {
        if (true)
        {
            while (false) { }
            for (int i = 0; i < 10; i++) { }
        }
    }
}";

        var context = new CSharpAnalyzerTest<AvoidComplexMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidComplexMethods)
            .WithSpan(6, 16, 6, 31) // Adjust the span based on the actual output
            .WithArguments("MyClass", 4); // Replace with actual complexity calculation
        
        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestConstructorComplexityBelowThreshold()
    {
        const string testCode = @"
public class MyClass
{
    public MyClass()
    {
        if (true) { }
    }
}";

        var context = new CSharpAnalyzerTest<AvoidComplexMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}