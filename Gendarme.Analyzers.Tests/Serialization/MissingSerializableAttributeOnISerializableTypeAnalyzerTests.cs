using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(MissingSerializableAttributeOnISerializableTypeAnalyzer))]
public sealed class MissingSerializableAttributeOnISerializableTypeAnalyzerTests
{
    [Fact]
    public async Task TestMissingSerializableAttribute()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;

[Serializable]
public class MySerializableClass { }

public class MyNonSerializableClass : ISerializable
{
    public MyNonSerializableClass() { }

    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}
";

        var context = new CSharpAnalyzerTest<MissingSerializableAttributeOnISerializableTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerError(DiagnosticId.MissingSerializableAttributeOnISerializableType)
            .WithSpan(8, 14, 8, 36)
            .WithArguments("MyNonSerializableClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}