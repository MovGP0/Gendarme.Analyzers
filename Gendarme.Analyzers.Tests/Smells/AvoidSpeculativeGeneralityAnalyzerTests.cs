using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidSpeculativeGeneralityAnalyzer))]
public sealed class AvoidSpeculativeGeneralityAnalyzerTests
{
    [Fact]
    public async Task TestAbstractClassWithSingleSubclass()
    {
        const string testCode = @"
public abstract class MyAbstractClass { }

public class SingleImplementation : MyAbstractClass { }
";

        var context = new CSharpAnalyzerTest<AvoidSpeculativeGeneralityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidSpeculativeGenerality)
            .WithSpan(1, 1, 5, 24)
            .WithArguments("MyAbstractClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAbstractClassWithMultipleSubclasses()
    {
        const string testCode = @"
public abstract class MyAbstractClass { }

public class FirstImplementation : MyAbstractClass { }
public class SecondImplementation : MyAbstractClass { }
";

        var context = new CSharpAnalyzerTest<AvoidSpeculativeGeneralityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected in this case.
        await context.RunAsync();
    }

    [Fact]
    public async Task TestConcreteClass()
    {
        const string testCode = @"
public class MyConcreteClass { }
";

        var context = new CSharpAnalyzerTest<AvoidSpeculativeGeneralityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for concrete classes.
        await context.RunAsync();
    }
}