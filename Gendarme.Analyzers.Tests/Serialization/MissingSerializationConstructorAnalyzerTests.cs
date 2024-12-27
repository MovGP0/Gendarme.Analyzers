using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(MissingSerializationConstructorAnalyzer))]
public sealed class MissingSerializationConstructorAnalyzerTests
{
    [Fact]
    public async Task TestMissingSerializationConstructor()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;

[Serializable]
public class MySerializableClass : ISerializable
{
    public MySerializableClass() { }

    // Missing serialization constructor
    // protected MySerializableClass(SerializationInfo info, StreamingContext context) { }
    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MissingSerializationConstructorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerError(DiagnosticId.MissingSerializationConstructor)
            .WithSpan(6, 14, 6, 33)
            .WithArguments("MySerializableClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidSerializationConstructor()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;

[Serializable]
public class MySerializableClass : ISerializable
{
    public MySerializableClass() { }

    protected MySerializableClass(SerializationInfo info, StreamingContext context) { }
    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}
";

        var context = new CSharpAnalyzerTest<MissingSerializationConstructorAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics as the constructor exists
        await context.RunAsync();
    }
}