using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(UseSingularNameInEnumsUnlessAreFlagsAnalyzer))]
public sealed class UseSingularNameInEnumsUnlessAreFlagsAnalyzerTests
{
    [Fact]
    public async Task TestEnumNameIsPluralWithoutFlags()
    {
        const string testCode = @"
public enum MyEnums
{
    Value1,
    Value2
}
";

        var context = new CSharpAnalyzerTest<UseSingularNameInEnumsUnlessAreFlagsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseSingularNameInEnumsUnlessAreFlags)
            .WithSpan(2, 13, 2, 20)
            .WithArguments("MyEnums");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEnumNameIsSingularWithFlags()
    {
        const string testCode = @"
using System;

[Flags]
public enum MyEnum
{
    Value1 = 1,
    Value2 = 2
}
";

        var context = new CSharpAnalyzerTest<UseSingularNameInEnumsUnlessAreFlagsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since the enum has the Flags attribute
        await context.RunAsync();
    }

    [Fact]
    public async Task TestEnumNameIsSingularCorrectly()
    {
        const string testCode = @"
public enum MyEnum
{
    SingleValue
}
";

        var context = new CSharpAnalyzerTest<UseSingularNameInEnumsUnlessAreFlagsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since the enum name is singular
        await context.RunAsync();
    }
}