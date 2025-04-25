using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ImplementEqualsAndGetHashCodeInPairAnalyzer))]
public sealed class ImplementEqualsAndGetHashCodeInPairAnalyzerTests
{
    [Fact]
    public async Task TestOnlyEqualsImplemented()
    {
        const string testCode = @"
public class MyClass
{
    public override bool Equals(object obj) => true;
}";

        var context = new CSharpAnalyzerTest<ImplementEqualsAndGetHashCodeInPairAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementEqualsAndGetHashCodeInPair)
            .WithSpan(2, 14, 2, 21)
            .WithArguments("MyClass", "Equals", "GetHashCode");
        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestOnlyGetHashCodeImplemented()
    {
        const string testCode = @"
public class MyClass
{
    public override int GetHashCode() => 1;
}";

        var context = new CSharpAnalyzerTest<ImplementEqualsAndGetHashCodeInPairAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementEqualsAndGetHashCodeInPair)
            .WithSpan(2, 14, 2, 21)
            .WithArguments("MyClass", "GetHashCode", "Equals");
        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestBothMethodsImplemented()
    {
        const string testCode = @"
public class MyClass
{
    public override bool Equals(object obj) => true;
    public override int GetHashCode() => 1;
}";

        var context = new CSharpAnalyzerTest<ImplementEqualsAndGetHashCodeInPairAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        context.ExpectedDiagnostics.Clear(); // No expected diagnostics since both methods are implemented.

        await context.RunAsync();
    }
}