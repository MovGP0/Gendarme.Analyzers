using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(PreferLiteralOverInitOnlyFieldsAnalyzer))]
public sealed class PreferLiteralOverInitOnlyFieldsAnalyzerTests
{
    [Fact]
    public async Task DetectsStaticReadonlyLiteralValue()
    {
        const string source = "public class Sample\n{\n    public static readonly int Value = 42;\n}\n";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(3, 32, 3, 37).WithArguments("Value"));

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsStaticReadonlyNonConstant()
    {
        const string source = "public class Sample\n{\n    public static readonly int Value = GetValue();\n\n    private static int GetValue() => 42;\n}\n";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsInstanceReadonly()
    {
        const string source = "public class Sample\n{\n    public readonly int Value = 42;\n}\n";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsStaticNonReadonly()
    {
        const string source = "public class Sample\n{\n    public static int Value = 42;\n}\n";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsConstField()
    {
        const string source = "public class Sample\n{\n    public const int Value = 42;\n}\n";

        await CreateTest(source).RunAsync();
    }

    private static CSharpAnalyzerTest<PreferLiteralOverInitOnlyFieldsAnalyzer, DefaultVerifier> CreateTest(string source) =>
        new()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

    private static DiagnosticResult Diagnostic() =>
        new(DiagnosticId.PreferLiteralOverInitOnlyFields, DiagnosticSeverity.Info);
}