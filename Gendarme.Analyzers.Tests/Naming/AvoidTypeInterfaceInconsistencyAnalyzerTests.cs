using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(AvoidTypeInterfaceInconsistencyAnalyzer))]
public sealed class AvoidTypeInterfaceInconsistencyAnalyzerTests
{
    [Fact]
    public async Task TestInterfaceClassInconsistency()
    {
        const string testCode = @"
public interface IMyService
{
    void DoSomething();
}

public class MyService  // Should warn because it doesn't implement IMyService
{
    public void DoSomethingElse() { }
}";

        var context = new CSharpAnalyzerTest<AvoidTypeInterfaceInconsistencyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidTypeInterfaceInconsistency)
            .WithSpan(7, 14, 7, 23)  // Point to class name
            .WithArguments("MyService", "IMyService");
        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestMatchingInterfaceImplementation()
    {
        const string testCode = @"
public interface IMyService
{
    void DoSomething();
}

public class MyService : IMyService  // No warning because it implements interface
{
    public void DoSomething() { }
}";

        var context = new CSharpAnalyzerTest<AvoidTypeInterfaceInconsistencyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}
