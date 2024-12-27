using Gendarme.Analyzers.Security.Cas;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(SecureGetObjectDataOverridesAnalyzer))]
public sealed class SecureGetObjectDataOverridesAnalyzerTests
{
    [Fact]
    public async Task TestGetObjectDataMethodWithoutSecurityAttribute()
    {
        const string testCode = @"
using System.Runtime.Serialization;

public class MySerializableClass : ISerializable
{
    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}";

        var context = new CSharpAnalyzerTest<SecureGetObjectDataOverridesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.SecureGetObjectDataOverrides)
            .WithSpan(6, 17, 6, 30)
            .WithArguments("MySerializableClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestGetObjectDataMethodWithSecurityAttributeAndPermission()
    {
        const string testCode = @"
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

public class MySerializableClass : ISerializable
{
    [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}";

        var context = new CSharpAnalyzerTest<SecureGetObjectDataOverridesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since the method is properly protected.
        
        await context.RunAsync();
    }

    [Fact]
    public async Task TestGetObjectDataMethodWithSecurityAttributeWithoutPermission()
    {
        const string testCode = @"
using System.Runtime.Serialization;
using System.Security.Permissions;

public class MySerializableClass : ISerializable
{
    [SecurityPermission(SecurityAction.Demand)]
    public void GetObjectData(SerializationInfo info, StreamingContext context) { }
}";

        var context = new CSharpAnalyzerTest<SecureGetObjectDataOverridesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.SecureGetObjectDataOverrides)
            .WithSpan(8, 17, 8, 30)
            .WithArguments("MySerializableClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}