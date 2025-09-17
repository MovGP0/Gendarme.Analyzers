using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ImplementICloneableCorrectlyAnalyzer))]
public sealed class ImplementICloneableCorrectlyAnalyzerTests
{
    [Fact]
    public async Task TestClassMissingICloneable()
    {
        const string testCode = @"
public class MyClass 
{
    public object Clone() 
    {
        return new MyClass();
    }
}
";

        var context = new CSharpAnalyzerTest<ImplementICloneableCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementICloneableCorrectly)
            .WithSpan(4, 19, 4, 24) // Update this span according to your code structure
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassImplementingICloneableCorrectly()
    {
        const string testCode = @"
using System;

public class MyCloneableClass : ICloneable
{
    public object Clone()
    {
        return new MyCloneableClass();
    }
}
";

        var context = new CSharpAnalyzerTest<ImplementICloneableCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since it implements ICloneable correctly
        await context.RunAsync();
    }
}