using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidTypeGetTypeForConstantStringsAnalyzer))]
public sealed class AvoidTypeGetTypeForConstantStringsAnalyzerTests
{
    [Fact]
    public async Task TestGetTypeForConstantString()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        Type type = Type.GetType(""System.String"");
    }
}";

        var context = new CSharpAnalyzerTest<AvoidTypeGetTypeForConstantStringsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidTypeGetTypeForConstantStrings)
            .WithSpan(8, 21, 8, 50)
            .WithArguments("System.String", "System.String");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestUsingTypeofInstead()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        Type type = typeof(string);
    }
}";

        var context = new CSharpAnalyzerTest<AvoidTypeGetTypeForConstantStringsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}