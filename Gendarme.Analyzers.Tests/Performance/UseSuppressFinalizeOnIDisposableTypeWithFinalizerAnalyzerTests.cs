using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzer))]
public sealed class UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzerTests
{
    [Fact]
    public async Task DetectsMissingSuppressFinalize()
    {
        const string source = """
using System;

public class Sample : IDisposable
{
    ~Sample()
    {
    }

    public void Dispose()
    {
    }
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(3, 14, 3, 20).WithArguments("Sample"));

        await test.RunAsync();
    }

    [Fact]
    public async Task DetectsSuppressFinalizeWithWrongInstance()
    {
        const string source = """
using System;

public class Sample : IDisposable
{
    private readonly object _other = new();

    ~Sample()
    {
    }

    public void Dispose()
    {
        GC.SuppressFinalize(_other);
    }
}
""";

        var test = CreateTest(source);
        test.ExpectedDiagnostics.Add(Diagnostic().WithSpan(3, 14, 3, 20).WithArguments("Sample"));

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsWhenSuppressFinalizeCalled()
    {
        const string source = """
using System;

public class Sample : IDisposable
{
    ~Sample()
    {
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
""";

        await CreateTest(source).RunAsync();
    }

    [Fact]
    public async Task SkipsWhenNoFinalizer()
    {
        const string source = """
using System;

public class Sample : IDisposable
{
    public void Dispose()
    {
    }
}
""";

        await CreateTest(source).RunAsync();
    }

    private static CSharpAnalyzerTest<UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzer, DefaultVerifier> CreateTest(string source) =>
        new()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

    private static DiagnosticResult Diagnostic() =>
        new(DiagnosticId.UseSuppressFinalizeOnIDisposableTypeWithFinalizer, DiagnosticSeverity.Warning);
}