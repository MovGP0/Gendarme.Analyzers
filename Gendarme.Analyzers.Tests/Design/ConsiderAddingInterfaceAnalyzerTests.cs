using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ConsiderAddingInterfaceAnalyzer))]
public sealed class ConsiderAddingInterfaceAnalyzerTests
{
    [Fact]
    public async Task TestTypeWithoutInterfaceButWithMethod_Do()
    {
        const string testCode = @"
public class MyClass
{
    public void Do() { }
}
";

        var context = new CSharpAnalyzerTest<ConsiderAddingInterfaceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.ConsiderAddingInterface, DiagnosticSeverity.Info)
            .WithSpan(4, 14, 4, 16)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestTypeWithInterfaceAndMethod_Do()
    {
        const string testCode = @"
public interface IDoable
{
    void Do();
}

public class MyClass : IDoable
{
    public void Do() { }
}
";

        var context = new CSharpAnalyzerTest<ConsiderAddingInterfaceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestTypeWithoutAnyMethod()
    {
        const string testCode = @"
public class MyClass
{
    public void AnotherMethod() { }
}
";

        var context = new CSharpAnalyzerTest<ConsiderAddingInterfaceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}