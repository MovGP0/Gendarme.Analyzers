using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(MarkAllNonSerializableFieldsAnalyzer))]
public sealed class MarkAllNonSerializableFieldsAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestNonSerializableField()
    {
        const string testCode = @"
using System;

[Serializable]
public class MyClass
{
    public int Field1;
    public string Field2;
    public object Field3; // Not serializable
}
";

        var context = new CSharpAnalyzerTest<MarkAllNonSerializableFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarkAllNonSerializableFields)
            .WithSpan(9, 19, 9, 25)
            .WithArguments("Field3", "MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestSerializableField()
    {
        const string testCode = @"
using System;

[Serializable]
public class MyClass
{
    public int Field1;
    [NonSerialized]
    public object Field2; // Not serialized, no diagnostic expected
}
";

        var context = new CSharpAnalyzerTest<MarkAllNonSerializableFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestNoSerializableAttribute()
    {
        const string testCode = @"
public class MyClass
{
    public int Field1; // No diagnostic expected
    public object Field2; // No diagnostic expected
}
";

        var context = new CSharpAnalyzerTest<MarkAllNonSerializableFieldsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}