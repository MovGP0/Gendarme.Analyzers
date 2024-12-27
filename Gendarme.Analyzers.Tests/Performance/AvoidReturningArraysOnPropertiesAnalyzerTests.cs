using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidReturningArraysOnPropertiesAnalyzer))]
public sealed class AvoidReturningArraysOnPropertiesAnalyzerTests
{
    [Fact]
    public async Task TestArrayProperty()
    {
        const string testCode = @"
namespace TestNamespace
{
    public class MyClass
    {
        public int[] MyProperty { get; set; }
    }
}";

        var context = new CSharpAnalyzerTest<AvoidReturningArraysOnPropertiesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidReturningArraysOnProperties)
            .WithSpan(6, 22, 6, 32)
            .WithArguments("MyProperty");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonArrayProperty()
    {
        const string testCode = @"
using System.Collections.Generic;

namespace TestNamespace
{
    public class MyClass
    {
        public List<int> MyProperty { get; set; }
    }
}";

        var context = new CSharpAnalyzerTest<AvoidReturningArraysOnPropertiesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for list properties.
        await context.RunAsync();
    }
}