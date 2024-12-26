using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(ToStringShouldNotReturnNullAnalyzer))]
public sealed class ToStringShouldNotReturnNullAnalyzerTests
{
    [Fact]
    public async Task TestToStringReturnsNull()
    {
        const string testCode = @"
public class MyClass
{
    public override string ToString()
    {
        return null;
    }
}";

        var context = new CSharpAnalyzerTest<ToStringShouldNotReturnNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ToStringShouldNotReturnNull)
            .WithSpan(6, 9, 6, 21);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestToStringReturnsNonNull()
    {
        const string testCode = @"
public class MyClass
{
    public override string ToString()
    {
        return ""Hello, World!"";
    }
}";

        var context = new CSharpAnalyzerTest<ToStringShouldNotReturnNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonToStringMethod()
    {
        const string testCode = @"
public class MyClass
{
    public string SomeMethod()
    {
        return null;
    }
}";

        var context = new CSharpAnalyzerTest<ToStringShouldNotReturnNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}