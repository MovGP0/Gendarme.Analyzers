using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(AvoidConstructorsInStaticTypesAnalyzer))]
public sealed class AvoidConstructorsInStaticTypesAnalyzerTests
{
    [Fact]
    public async Task ReportsWhenStaticLikeTypeHasPublicConstructor()
    {
        const string source = @"
public class Utilities
{
    public static void DoWork() { }

    public Utilities() { }
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidConstructorsInStaticTypes)
            .WithSpan(6, 12, 6, 21)
            .WithArguments("Utilities");

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task ReportsWhenStaticLikeTypeHasProtectedConstructor()
    {
        const string source = @"
public class BaseUtilities
{
    public static void DoWork() { }

    protected BaseUtilities() { }
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidConstructorsInStaticTypes)
            .WithSpan(6, 15, 6, 28)
            .WithArguments("BaseUtilities");

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task SkipsWhenConstructorIsPrivate()
    {
        const string source = @"
public class HiddenUtilities
{
    public static void DoWork() { }

    private HiddenUtilities() { }
}
";

        await VerifyAsync(source);
    }

    [Fact]
    public async Task SkipsWhenTypeHasInstanceMembers()
    {
        const string source = @"
public class MixedUtilities
{
    public static void DoWork() { }

    public MixedUtilities() { }

    public int Value { get; } = 42;
}
";

        await VerifyAsync(source);
    }

    [Fact]
    public async Task SkipsStaticClassWithStaticConstructor()
    {
        const string source = @"
public static class StaticUtilities
{
    static StaticUtilities() { }
}
";

        await VerifyAsync(source);
    }

    private static Task VerifyAsync(string source, params DiagnosticResult[] expectedDiagnostics)
    {
        var test = new CSharpAnalyzerTest<AvoidConstructorsInStaticTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        return test.RunAsync();
    }
}
