using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(DoNotUseEnumIsAssignableFromAnalyzer))]
public sealed class DoNotUseEnumIsAssignableFromAnalyzerTests
{
    [Fact]
    public async Task TestEnumIsAssignableFromUsage()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void CheckEnum()
    {
        bool result = typeof(Enum).IsAssignableFrom(typeof(MyEnum));
    }

    private enum MyEnum { Value1, Value2 }
}
";

        var context = new CSharpAnalyzerTest<DoNotUseEnumIsAssignableFromAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotUseEnumIsAssignableFrom)
            .WithSpan(6, 20, 6, 44)
            .WithArguments("typeof(MyEnum)");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidUsageOfIsAssignableFrom()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void CheckType()
    {
        bool result = typeof(object).IsAssignableFrom(typeof(MyEnum));
    }

    private enum MyEnum { Value1, Value2 }
}
";

        var context = new CSharpAnalyzerTest<DoNotUseEnumIsAssignableFromAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}