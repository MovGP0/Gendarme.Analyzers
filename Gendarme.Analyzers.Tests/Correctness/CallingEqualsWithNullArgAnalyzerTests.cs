using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(CallingEqualsWithNullArgAnalyzer))]
public sealed class CallingEqualsWithNullArgAnalyzerTests
{
    [Fact]
    public async Task TestEqualsWithNullArgument()
    {
        const string testCode = @"
public class MyClass
{
    public override bool Equals(object obj) => base.Equals(obj);
}

class TestClass
{
    public void TestMethod()
    {
        var myClass = new MyClass();
        myClass.Equals(null);
    }
}";

        var context = new CSharpAnalyzerTest<CallingEqualsWithNullArgAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.CallingEqualsWithNullArg)
            .WithSpan(12, 9, 12, 29)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNotEqualsMethod()
    {
        const string testCode = @"
public class MyClass
{
    public bool NotEquals(object obj) => false;
}

class TestClass
{
    public void TestMethod()
    {
        var myClass = new MyClass();
        myClass.NotEquals(null);
    }
}";

        var context = new CSharpAnalyzerTest<CallingEqualsWithNullArgAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected as the method is not Equals
        await context.RunAsync();
    }
}