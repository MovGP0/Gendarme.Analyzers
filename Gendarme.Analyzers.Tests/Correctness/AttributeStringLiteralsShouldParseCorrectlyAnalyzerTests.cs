using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(AttributeStringLiteralsShouldParseCorrectlyAnalyzer))]
public sealed class AttributeStringLiteralsShouldParseCorrectlyAnalyzerTests
{
    [Fact]
    public async Task DetectsInvalidVersionLiteral()
    {
        const string source = @"using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class VersionLikeAttribute : Attribute
{
    public VersionLikeAttribute(string version) { }
}

[VersionLikeAttribute(""not.a.valid.version"")]
public sealed class C { }
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AttributeStringLiteralsShouldParseCorrectly)
            .WithSpan(9, 23, 9, 44)
            .WithArguments("not.a.valid.version");

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task DetectsInvalidGuidLiteral()
    {
        const string source = @"using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class GuidLikeAttribute : Attribute
{
    public GuidLikeAttribute(string guid) { }
}

[GuidLikeAttribute(""invalid-guid"")]
public sealed class C { }
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AttributeStringLiteralsShouldParseCorrectly)
            .WithSpan(9, 20, 9, 34)
            .WithArguments("invalid-guid");

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task DetectsInvalidUriLiteral()
    {
        const string source = @"using System;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class UriLikeAttribute : Attribute
{
    public UriLikeAttribute(string uri) { }
}

[UriLikeAttribute(""http://invalid url"")]
public sealed class C { }
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AttributeStringLiteralsShouldParseCorrectly)
            .WithSpan(9, 19, 9, 39)
            .WithArguments("http://invalid url");

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task SkipsWhenParameterHasStringSyntaxAttribute()
    {
        const string source = @"using System;
using System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Class)]
public sealed class UriAttribute : Attribute
{
    public UriAttribute([StringSyntax(StringSyntaxAttribute.Uri)] string uri) { }
}

[UriAttribute(""http://invalid url"")]
public sealed class C { }
";

        await VerifyAsync(source);
    }

    [Fact]
    public async Task SkipsWhenPropertyHasStringSyntaxAttribute()
    {
        const string source = @"using System;
using System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Class)]
public sealed class UriAttribute : Attribute
{
    [StringSyntax(StringSyntaxAttribute.Uri)]
    public string Uri { get; set; }
}

[UriAttribute(Uri = ""http://invalid url"")]
public sealed class C { }
";

        await VerifyAsync(source);
    }

    private static Task VerifyAsync(string source, params DiagnosticResult[] expectedDiagnostics)
    {
        var test = new CSharpAnalyzerTest<AttributeStringLiteralsShouldParseCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        return test.RunAsync();
    }
}
