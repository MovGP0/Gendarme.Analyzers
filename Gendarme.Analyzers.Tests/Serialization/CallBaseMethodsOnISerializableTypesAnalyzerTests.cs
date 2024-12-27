using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(CallBaseMethodsOnISerializableTypesAnalyzer))]
public sealed class CallBaseMethodsOnISerializableTypesAnalyzerTests
{
    [Fact]
    public async Task TestBaseConstructorNotCalled()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;

[Serializable]
public class MySerializableBaseClass : ISerializable
{
    public MySerializableBaseClass(SerializationInfo info, StreamingContext context) { }
    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}

[Serializable]
public class MySerializableChildClass : MySerializableBaseClass
{
    public MySerializableChildClass(SerializationInfo info, StreamingContext context) : base(info, context) { }
    public new void GetObjectData(SerializationInfo info, StreamingContext context) { }
}
";

        var context = new CSharpAnalyzerTest<CallBaseMethodsOnISerializableTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.CallBaseMethodsOnISerializableTypes)
            .WithSpan(16, 21, 16, 34)
            .WithArguments("MySerializableChildClass", "GetObjectData");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoBaseObject()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;

[Serializable]
public class MySerializableClass : ISerializable
{
    public MySerializableClass(SerializationInfo info, StreamingContext context) { }

    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}
";

        var context = new CSharpAnalyzerTest<CallBaseMethodsOnISerializableTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestBaseMethodsCalled()
    {
        const string testCode =@"
using System;
using System.Runtime.Serialization;

[Serializable]
public class MySerializableBaseClass : ISerializable
{
    public MySerializableBaseClass(SerializationInfo info, StreamingContext context) { }
    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}

[Serializable]
public class MySerializableChildClass : MySerializableBaseClass
{
    public MySerializableChildClass(SerializationInfo info, StreamingContext context) : base(info, context) { }
    public new void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
    }
}
";

        var context = new CSharpAnalyzerTest<CallBaseMethodsOnISerializableTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since both base methods are called.
        
        await context.RunAsync();
    }
}