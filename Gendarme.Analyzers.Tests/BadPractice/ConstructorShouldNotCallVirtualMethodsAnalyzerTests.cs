using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(ConstructorShouldNotCallVirtualMethodsAnalyzer))]
public sealed class ConstructorShouldNotCallVirtualMethodsAnalyzerTests
{
    [Fact]
    public async Task TestConstructorCallingVirtualMethod()
    {
        const string testCode = @"
public class BaseClass
{
    public BaseClass()
    {
        VirtualMethod();
    }

    protected virtual void VirtualMethod() { }
}

public class DerivedClass : BaseClass
{
    protected override void VirtualMethod() { }
}
";

        var context = new CSharpAnalyzerTest<ConstructorShouldNotCallVirtualMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ConstructorShouldNotCallVirtualMethods)
            .WithSpan(6, 9, 6, 24) // adjust the span to match the line where the virtual method is called
            .WithArguments("VirtualMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestConstructorNotCallingVirtualMethod()
    {
        const string testCode = @"
public class BaseClass
{
    public BaseClass()
    {
        // No virtual method call here
    }
}

public class DerivedClass : BaseClass { }
";

        var context = new CSharpAnalyzerTest<ConstructorShouldNotCallVirtualMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}