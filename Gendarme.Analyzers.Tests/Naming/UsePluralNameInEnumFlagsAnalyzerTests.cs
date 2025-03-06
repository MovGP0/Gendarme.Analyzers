using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(UsePluralNameInEnumFlagsAnalyzer))]
public sealed class UsePluralNameInEnumFlagsAnalyzerTests
{
    [Fact]
    public async Task TestEnumWithFlagsAttributeAndPluralName()
    {
        const string testCode = @"
using System;

[Flags]
public enum MyEnums
{
    Value1,
    Value2
}
";
        var context = new CSharpAnalyzerTest<UsePluralNameInEnumFlagsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics are expected since the enum name is plural.
        await context.RunAsync();
    }

    [Fact]
    public async Task TestEnumWithFlagsAttributeAndSingularName()
    {
        const string testCode = @"
using System;

[Flags]
public enum MyEnum
{
    Value1,
    Value2
}
";
        var context = new CSharpAnalyzerTest<UsePluralNameInEnumFlagsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UsePluralNameInEnumFlags)
            .WithSpan(5, 13, 5, 19)
            .WithArguments("MyEnum");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonFlagsEnum()
    {
        const string testCode = @"
public enum NonFlagsEnum
{
    Value1,
    Value2
}
";
        var context = new CSharpAnalyzerTest<UsePluralNameInEnumFlagsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics are expected since it's not a flags enum.
        await context.RunAsync();
    }
}