using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(PreferEmptyInstanceOverNullAnalyzer))]
public sealed class PreferEmptyInstanceOverNullAnalyzerTests
{
    [Fact]
    public async Task TestMethodReturningNull()
    {
        const string testCode = @"
using System.Collections.Generic;

public class MyClass
{
    public List<string> GetList()
    {
        return null; // This should trigger a diagnostic
    }
}
";

        var context = new CSharpAnalyzerTest<PreferEmptyInstanceOverNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferEmptyInstanceOverNull)
            .WithSpan(8, 9, 8, 21)
            .WithArguments("GetList");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPropertyReturningNull()
    {
        const string testCode = @"
using System.Collections.Generic;

public class MyClass
{
    public List<string> MyList
    {
        get { return null; } // This should also trigger a diagnostic
    }
}
";

        var context = new CSharpAnalyzerTest<PreferEmptyInstanceOverNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferEmptyInstanceOverNull)
            .WithSpan(8, 15, 8, 27)
            .WithArguments("MyList");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodReturningEmptyList()
    {
        const string testCode = @"
using System.Collections.Generic;

public class MyClass
{
    public List<string> GetList()
    {
        return new List<string>(); // This should NOT trigger a diagnostic
    }
}
";

        var context = new CSharpAnalyzerTest<PreferEmptyInstanceOverNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestPropertyReturningEmptyList()
    {
        const string testCode = @"
using System.Collections.Generic;

public class MyClass
{
    public List<string> MyList
    {
        get { return new List<string>(); } // This should NOT trigger a diagnostic
    }
}
";

        var context = new CSharpAnalyzerTest<PreferEmptyInstanceOverNullAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}