using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(DeserializeOptionalFieldAnalyzer))]
public sealed class DeserializeOptionalFieldAnalyzerTests
{
    [Fact]
    public async Task TestMissingDeserializationMethod()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;

[Serializable]
public class MyClass
{
    [OptionalField]
    public int OptionalField;

    // Missing OnDeserialized or OnDeserializing method
}
";

        var context = new CSharpAnalyzerTest<DeserializeOptionalFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DeserializeOptionalField)
            .WithSpan(6, 14, 6, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestWithDeserializationMethod()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;

[Serializable]
public class MyClass
{
    [OptionalField]
    public int OptionalField;

    [OnDeserialized]
    public void OnDeserializedMethod(StreamingContext context) { }
}
";

        var context = new CSharpAnalyzerTest<DeserializeOptionalFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected in this case
        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassWithoutOptionalField()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;

[Serializable]
public class MyClass
{
    public int RegularField;
}
";

        var context = new CSharpAnalyzerTest<DeserializeOptionalFieldAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected in this case
        await context.RunAsync();
    }
}