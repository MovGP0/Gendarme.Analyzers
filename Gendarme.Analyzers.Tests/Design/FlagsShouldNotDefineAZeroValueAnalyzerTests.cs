using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(FlagsShouldNotDefineAZeroValueAnalyzer))]
public sealed class FlagsShouldNotDefineAZeroValueAnalyzerTests
{
    [Fact]
    public async Task TestFlagsWithZeroValue()
    {
        const string testCode = @"
using System;

[Flags]
public enum MyFlags 
{
    None = 0,
    Option1 = 1,
    Option2 = 2
}
";

        var context = new CSharpAnalyzerTest<FlagsShouldNotDefineAZeroValueAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.FlagsShouldNotDefineAZeroValue)
            .WithSpan(5, 13, 5, 20)
            .WithArguments("MyFlags");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestFlagsWithoutZeroValue()
    {
        const string testCode = @"
using System;

[Flags]
public enum MyFlags 
{
    Option1 = 1,
    Option2 = 2
}
";

        var context = new CSharpAnalyzerTest<FlagsShouldNotDefineAZeroValueAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        context.ExpectedDiagnostics.Clear(); // No diagnostics expected

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonFlagsEnum()
    {
        const string testCode = @"
public enum NonFlagsEnum 
{
    Value1 = 1,
    Value2 = 2
}
";

        var context = new CSharpAnalyzerTest<FlagsShouldNotDefineAZeroValueAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        context.ExpectedDiagnostics.Clear(); // No diagnostics expected

        await context.RunAsync();
    }
}