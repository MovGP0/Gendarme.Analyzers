using Gendarme.Analyzers.Serialization;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(MarkEnumerationsAsSerializableAnalyzer))]
public sealed class MarkEnumerationsAsSerializableAnalyzerTests
{
    [Fact]
    public async Task TestEnumWithoutSerializableAttribute()
    {
        const string testCode = @"
public enum MyEnum { Value1, Value2 }
";

        var context = new CSharpAnalyzerTest<MarkEnumerationsAsSerializableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.MarkEnumerationsAsSerializable, DiagnosticSeverity.Info)
            .WithSpan(2, 13, 2, 19)
            .WithArguments("MyEnum");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEnumWithSerializableAttribute()
    {
        const string testCode = @"
[System.Serializable]
public enum MySerializableEnum { Value1, Value2 }
";

        var context = new CSharpAnalyzerTest<MarkEnumerationsAsSerializableAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}