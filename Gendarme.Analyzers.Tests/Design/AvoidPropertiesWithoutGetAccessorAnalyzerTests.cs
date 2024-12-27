using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AvoidPropertiesWithoutGetAccessorAnalyzer))]
public sealed class AvoidPropertiesWithoutGetAccessorAnalyzerTests
{
    [Fact]
    public async Task TestPropertyWithSetOnly()
    {
        const string testCode = @"
public class MyClass
{
    public string MyProperty { set; private get; }
}";

        var context = new CSharpAnalyzerTest<AvoidPropertiesWithoutGetAccessorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidPropertiesWithoutGetAccessor)
            .WithSpan(4, 22, 4, 36)
            .WithArguments("MyProperty");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPropertyWithGetAndSet()
    {
        const string testCode = @"
public class MyClass
{
    public string MyProperty { get; set; }
}";

        var context = new CSharpAnalyzerTest<AvoidPropertiesWithoutGetAccessorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPrivatePropertyWithSetOnly()
    {
        const string testCode = @"
class MyClass
{
    public string MyProperty { set; private get; }
}";

        var context = new CSharpAnalyzerTest<AvoidPropertiesWithoutGetAccessorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInternalPropertyWithSetOnly()
    {
        const string testCode = @"
internal class MyClass
{
    internal string MyProperty { set; private get; }
}";

        var context = new CSharpAnalyzerTest<AvoidPropertiesWithoutGetAccessorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}