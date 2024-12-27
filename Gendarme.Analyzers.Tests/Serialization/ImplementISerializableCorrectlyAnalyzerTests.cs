using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(ImplementISerializableCorrectlyAnalyzer))]
public sealed class ImplementISerializableCorrectlyAnalyzerTests
{
    [Fact]
    public async Task TestGetObjectDataNotVirtualForNonSealedClass()
    {
        const string testCode = @"
using System.Runtime.Serialization;

[System.Serializable]
public class MySerializableClass : ISerializable
{
    public int Field1;
    public string Field2;

    public MySerializableClass() { }

    // This method should be virtual; otherwise, it should trigger a warning.
    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}
";

        var context = new CSharpAnalyzerTest<ImplementISerializableCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementISerializableCorrectly)
            .WithSpan(13, 17, 13, 30)
            .WithArguments("MySerializableClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonSerializedField()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;

[System.Serializable]
public class MyClass : ISerializable
{
    public int Field1;
    
    [NonSerialized]
    public string Field2;

    public MyClass() { }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(""Field1"", Field1);
    }
}
";

        var context = new CSharpAnalyzerTest<ImplementISerializableCorrectlyAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementISerializableCorrectly)
            .WithSpan(15, 17, 15, 30)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}