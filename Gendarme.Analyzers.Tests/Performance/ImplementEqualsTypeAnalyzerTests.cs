using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(ImplementEqualsTypeAnalyzer))]
public sealed class ImplementEqualsTypeAnalyzerTests
{
    [Fact]
    public async Task DetectsMissingTypeSpecificEqualsAndInterface()
    {
        const string source = """
public class Sample
{
    public override bool Equals(object? obj) => base.Equals(obj);
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(1, 14, 1, 20).WithArguments("Sample"));

        await test.RunAsync();
    }

    [Fact]
    public async Task DetectsMissingInterface()
    {
        const string source = """
public class Sample
{
    public override bool Equals(object? obj) => base.Equals(obj);

    public bool Equals(Sample other) => other is not null;
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(1, 14, 1, 20).WithArguments("Sample"));

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsWhenRequirementsMet()
    {
        const string source = """
using System;

public class Sample : IEquatable<Sample>
{
    public override bool Equals(object? obj) => obj is Sample other && Equals(other);

    public bool Equals(Sample other) => other is not null;
}
""";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsWhenDoesNotOverrideEquals()
    {
        const string source = """
using System;

public class Sample : IEquatable<Sample>
{
    public bool Equals(Sample other) => other is not null;
}
""";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsForGenericType()
    {
        const string source = """
using System;

public class Sample<T> : IEquatable<Sample<T>>
{
    public override bool Equals(object? obj) => obj is Sample<T> other && Equals(other);

    public bool Equals(Sample<T> other) => other is not null;
}
""";

        await CreateTest(source).RunAsync();
    }

    private static CSharpAnalyzerTest<ImplementEqualsTypeAnalyzer, DefaultVerifier> CreateTest(string source) =>
        new()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

    private static DiagnosticResult Diagnostic() =>
        new(DiagnosticId.ImplementEqualsType, DiagnosticSeverity.Info);
}