using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidLackOfCohesionOfMethodsAnalyzer))]
public sealed class AvoidLackOfCohesionOfMethodsAnalyzerTests
{
    [Fact]
    public async Task TestLackOfCohesionIdentified()
    {
        const string testCode = @"
public class MyClass
{
    private int field1;
    private int field2;
    private int field3;
    private int field4;
    private int field5;

    public void Method1() { field1 = 1; }
    public void Method2() { field2 = 2; }
    public void Method3() { field3 = 3; }
    public void Method4() { field4 = 4; }
    public void Method5() { field5 = 5; }
}
";

        var context = new CSharpAnalyzerTest<AvoidLackOfCohesionOfMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidLackOfCohesionOfMethods)
            .WithSpan(2, 14, 2, 21)
            .WithArguments("MyClass", "1,00");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCohesionNotIdentified()
    {
        const string testCode = @"
public class MyClass
{
    private int field1;
    private int field2;
    private int field3;
    private int field4;
    private int field5;

    public void Method1() { field1 = 1; }
    public void Method2() { field1 = 2; }
    public void Method3() { field1 = 3; }
    public void Method4() { field1 = 4; }
    public void Method5() { field1 = 5; }
}
";

        var context = new CSharpAnalyzerTest<AvoidLackOfCohesionOfMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}