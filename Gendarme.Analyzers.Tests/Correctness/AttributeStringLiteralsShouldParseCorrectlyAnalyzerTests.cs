using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(AttributeStringLiteralsShouldParseCorrectlyAnalyzer))]
public sealed class AttributeStringLiteralsShouldParseCorrectlyAnalyzerTests
{
    [Fact]
    public async Task TestInvalidVersionAttribute()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MyAttribute : Attribute
{
    public MyAttribute(string version) { }
}

[MyAttribute(""not.a.valid.version"")]
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<AttributeStringLiteralsShouldParseCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AttributeStringLiteralsShouldParseCorrectly)
            .WithSpan(6, 15, 6, 38)
            .WithArguments("not.a.valid.version");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvalidGuidAttribute()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MyAttribute : Attribute
{
    public MyAttribute(string guid) { }
}

[MyAttribute(""invalid-guid"")]
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<AttributeStringLiteralsShouldParseCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AttributeStringLiteralsShouldParseCorrectly)
            .WithSpan(6, 15, 6, 32)
            .WithArguments("invalid-guid");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvalidUriAttribute()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MyAttribute : Attribute
{
    public MyAttribute(string uri) { }
}

[MyAttribute(""http://invalid-url"")]
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<AttributeStringLiteralsShouldParseCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AttributeStringLiteralsShouldParseCorrectly)
            .WithSpan(6, 15, 6, 33)
            .WithArguments("http://invalid-url");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}