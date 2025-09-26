using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUnusedPrivateFieldsAnalyzer))]
public sealed class AvoidUnusedPrivateFieldsAnalyzerTests
{
    [Fact]
    public async Task DetectsUnusedPrivateField()
    {
        const string source = """
public class MyClass
{
    private int unusedField;

    public void M()
    {
    }
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(3, 17, 3, 28).WithArguments("unusedField"));

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsUsedPrivateField()
    {
        const string source = """
public class MyClass
{
    private int usedField;

    public void M()
    {
        usedField = 5;
    }
}
""";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsConstField()
    {
        const string source = """
public class MyClass
{
    private const int ConstField = 5;

    public int Value => ConstField;
}
""";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsFieldUsedFromNestedType()
    {
        const string source = """
public class Outer
{
    private int _value;

    private sealed class Inner
    {
        public void Use(Outer outer) => outer._value = 42;
    }

    public void EnsureUsage()
    {
        new Inner().Use(this);
    }
}
""";

        await CreateTest(source).RunAsync();
    }

    private static CSharpAnalyzerTest<AvoidUnusedPrivateFieldsAnalyzer, DefaultVerifier> CreateTest(string source) =>
        new()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

    private static DiagnosticResult Diagnostic() =>
        new(DiagnosticId.AvoidUnusedPrivateFields, DiagnosticSeverity.Info);
}