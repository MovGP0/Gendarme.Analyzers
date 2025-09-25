using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUnusedParametersAnalyzer))]
public sealed class AvoidUnusedParametersAnalyzerTests
{
    [Fact]
    public async Task TestUnusedParameterWarning()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod(int unusedParameter)
    {
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnusedParametersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidUnusedParameters)
            .WithSpan(4, 24, 4, 41)
            .WithArguments("unusedParameter", "MyMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestUsedParameterNoWarning()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod(int usedParameter)
    {
        Console.WriteLine(usedParameter);
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnusedParametersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestEventHandlerNoWarning()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyEventHandler(object sender, EventArgs e)
    {
        // Handler logic here
    }
}";

        var context = new CSharpAnalyzerTest<AvoidUnusedParametersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}