using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AttributeArgumentsShouldHaveAccessorsAnalyzer))]
public sealed class AttributeArgumentsShouldHaveAccessorsAnalyzerTests
{
    [Fact]
    public async Task TestAttributeConstructorWithoutAccessors()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MyAttribute : Attribute
{
    public MyAttribute(string value) {}
}

[MyAttribute(""Some Value"")]
public class MyClass {}
";

        var context = new CSharpAnalyzerTest<AttributeArgumentsShouldHaveAccessorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AttributeArgumentsShouldHaveAccessors)
            .WithSpan(8, 1, 8, 11) // Change as per exact span generated
            .WithArguments("MyAttribute", "value");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAttributeConstructorWithAccessors()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MyAttribute : Attribute
{
    public string Value { get; }

    public MyAttribute(string value)
    {
        Value = value;
    }
}

[MyAttribute(""Some Value"")]
public class MyClass {}
";

        var context = new CSharpAnalyzerTest<AttributeArgumentsShouldHaveAccessorsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}