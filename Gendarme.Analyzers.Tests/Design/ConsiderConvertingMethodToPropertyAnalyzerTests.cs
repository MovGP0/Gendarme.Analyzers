using Gendarme.Analyzers.Design;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ConsiderConvertingMethodToPropertyAnalyzer))]
public sealed class ConsiderConvertingMethodToPropertyAnalyzerTests
{
    [Fact]
    public async Task TestMethodSuggestsConversionToProperty()
    {
        const string testCode = @"
public class MyClass
{
    public int GetValue() => 42;
}
";

        var context = new CSharpAnalyzerTest<ConsiderConvertingMethodToPropertyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.ConsiderConvertingMethodToProperty, DiagnosticSeverity.Info)
            .WithSpan(4, 26, 4, 34)
            .WithArguments("GetValue");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestMethodDoesNotSuggestConversionWhenParametersPresent()
    {
        const string testCode = @"
public class MyClass
{
    public int GetValue(int input) => input;
}
";

        var context = new CSharpAnalyzerTest<ConsiderConvertingMethodToPropertyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodDoesNotSuggestConversionWhenReturnsVoid()
    {
        const string testCode = @"
public class MyClass
{
    public void DoSomething() { }
}
";

        var context = new CSharpAnalyzerTest<ConsiderConvertingMethodToPropertyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodSuggestsConversionWhenMethodIsShort()
    {
        const string testCode = @"
public class MyClass
{
    public bool IsReady() => true;
}
";

        var context = new CSharpAnalyzerTest<ConsiderConvertingMethodToPropertyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.ConsiderConvertingMethodToProperty, DiagnosticSeverity.Info)
            .WithSpan(4, 26, 4, 34)
            .WithArguments("IsReady");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}