using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(UseCorrectSignatureForSerializationMethodsAnalyzer))]
public sealed class UseCorrectSignatureForSerializationMethodsAnalyzerTests
{
    [Fact]
    public async Task TestInvalidSignature_NoAttribute()
    {
        const string testCode = @"
    public class MyClass
    {
        public void NotSerializedMethod() { }
    }";

        var context = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvalidSignature_WithAttribute()
    {
        const string testCode = @"
    using System.Runtime.Serialization;

    public class MyClass
    {
        [OnSerialized]
        public void OnSerializedMethod(string value) { }

        [OnSerialized]
        public void OnSerializedMethod(StreamingContext context, int extraParameter) { }
    }";

        var context = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = DiagnosticResult
            .CompilerError(DiagnosticId.UseCorrectSignatureForSerializationMethods)
            .WithSpan(7, 21, 7, 39)
            .WithArguments("OnSerializedMethod");

        context.ExpectedDiagnostics.Add(expected1);

        var expected2 = DiagnosticResult
            .CompilerError(DiagnosticId.UseCorrectSignatureForSerializationMethods)
            .WithSpan(10, 21, 10, 39)
            .WithArguments("OnSerializedMethod");

        context.ExpectedDiagnostics.Add(expected2);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidSignature()
    {
        const string testCode = @"
    using System.Runtime.Serialization;

    public class MyClass
    {
        [OnSerialized]
        public void OnSerializedMethod(StreamingContext context) { }
    }";

        var context = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}