using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(CloneMethodShouldNotReturnNullAnalyzer))]
public sealed class CloneMethodShouldNotReturnNullAnalyzerTests
{
    [Fact]
    public async Task TestCloneMethodReturnsNull()
    {
        const string testCode = @"
public class MyClass : System.ICloneable
{
    public object Clone()
    {
        return null;
    }
}
";

        var context = new CSharpAnalyzerTest<CloneMethodShouldNotReturnNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.CloneMethodShouldNotReturnNull)
            .WithSpan(6, 16, 6, 20);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCloneMethodReturnsNonNullable()
    {
        const string testCode = @"
public class MyClass : System.ICloneable
{
    public object Clone()
    {
        return new MyClass();
    }
}
";

        var context = new CSharpAnalyzerTest<CloneMethodShouldNotReturnNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // no diagnostics expected
    }

    [Fact]
    public async Task TestNonCloneMethod()
    {
        const string testCode = @"
public class MyOtherClass
{
    public object Clone() { return null; }
}
";

        var context = new CSharpAnalyzerTest<CloneMethodShouldNotReturnNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // no diagnostics expected
    }

    [Fact]
    public async Task TestCloneMethodWithParameters()
    {
        const string testCode = @"
public class MyClass : System.ICloneable
{
    public object Clone()
    {
        return new MyClass();
    }

    public object Clone(int param)
    {
        return null;
    }
}
";

        var context = new CSharpAnalyzerTest<CloneMethodShouldNotReturnNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };
        
        await context.RunAsync(); // no diagnostics expected
    }
}