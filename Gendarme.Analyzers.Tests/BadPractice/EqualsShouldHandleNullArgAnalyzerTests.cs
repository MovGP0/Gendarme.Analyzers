using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(EqualsShouldHandleNullArgAnalyzer))]
public sealed class EqualsShouldHandleNullArgAnalyzerTests
{
    [Fact]
    public async Task TestEqualsMethodWithoutNullCheck()
    {
        const string testCode = @"
public class MyClass
{
    public bool Equals(object obj)
    {
        return true; // No null check
    }
}
";

        var context = new CSharpAnalyzerTest<EqualsShouldHandleNullArgAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.EqualsShouldHandleNullArg)
            .WithSpan(4, 17, 4, 23);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEqualsMethodWithNullCheck()
    {
        const string testCode = @"
public class MyClass
{
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;
        return true; // Null check present
    }
}
";

        var context = new CSharpAnalyzerTest<EqualsShouldHandleNullArgAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        context.ExpectedDiagnostics.Clear(); // No diagnostics expected

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonEqualsMethod()
    {
        const string testCode = @"
public class MyClass
{
    public bool Compare(object obj)
    {
        return true; // Not an Equals method
    }
}
";

        var context = new CSharpAnalyzerTest<EqualsShouldHandleNullArgAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        context.ExpectedDiagnostics.Clear(); // No diagnostics expected

        await context.RunAsync();
    }
}