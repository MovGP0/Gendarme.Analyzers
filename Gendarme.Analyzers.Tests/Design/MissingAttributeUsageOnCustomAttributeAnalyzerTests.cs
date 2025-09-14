using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(MissingAttributeUsageOnCustomAttributeAnalyzer))]
public sealed class MissingAttributeUsageOnCustomAttributeAnalyzerTests
{
    [Fact]
    public async Task TestMissingAttributeUsage()
    {
        const string testCode = @"
using System;

public class MyCustomAttribute : Attribute { }

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MissingAttributeUsageOnCustomAttributeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MissingAttributeUsageOnCustomAttribute)
            .WithSpan(4, 14, 4, 31)
            .WithArguments("MyCustomAttribute");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCorrectAttributeUsage()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MyCustomAttribute : Attribute { }

[MyCustomAttribute]
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MissingAttributeUsageOnCustomAttributeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since the MyCustomAttribute has the correct AttributeUsage
        await context.RunAsync();
    }
}