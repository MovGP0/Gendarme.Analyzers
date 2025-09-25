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
        private void OnSerializedMethod(StreamingContext context) { }
    }";

        var context = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAllAttributesValidSignature()
    {
        const string testCode = @"
    using System.Runtime.Serialization;

    public class MyClass
    {
        [OnSerializing]
        private void OnSerializingMethod(StreamingContext context) { }

        [OnDeserializing]
        private void OnDeserializingMethod(StreamingContext context) { }

        [OnSerialized]
        private void OnSerializedMethod(StreamingContext context) { }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context) { }
    }";

        var test = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task TestWrongVisibility_ShouldReport()
    {
        const string testCode = @"
    using System.Runtime.Serialization;

    public class MyClass
    {
        [OnDeserialized]
        public void Handler(StreamingContext context) { }
    }";

        var test = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerError(DiagnosticId.UseCorrectSignatureForSerializationMethods)
            .WithSpan(7, 21, 7, 28)
            .WithArguments("Handler");

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task TestWrongReturnType_ShouldReport()
    {
        const string testCode = @"
    using System.Runtime.Serialization;

    public class MyClass
    {
        [OnSerialized]
        private int Handler(StreamingContext context) => 0;
    }";

        var test = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerError(DiagnosticId.UseCorrectSignatureForSerializationMethods)
            .WithSpan(7, 21, 7, 28)
            .WithArguments("Handler");

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task TestWrongParameterType_ShouldReport()
    {
        const string testCode = @"
    using System.Runtime.Serialization;

    public class MyClass
    {
        [OnSerializing]
        private void Handler(int notContext) { }
    }";

        var test = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerError(DiagnosticId.UseCorrectSignatureForSerializationMethods)
            .WithSpan(7, 22, 7, 29)
            .WithArguments("Handler");

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task TestNoParameter_ShouldReport()
    {
        const string testCode = @"
    using System.Runtime.Serialization;

    public class MyClass
    {
        [OnDeserializing]
        private void Handler() { }
    }";

        var test = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerError(DiagnosticId.UseCorrectSignatureForSerializationMethods)
            .WithSpan(7, 22, 7, 29)
            .WithArguments("Handler");

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task TestTwoParameters_ShouldReport()
    {
        const string testCode = @"
    using System.Runtime.Serialization;

    public class MyClass
    {
        [OnDeserialized]
        private void Handler(StreamingContext context, int extra) { }
    }";

        var test = new CSharpAnalyzerTest<UseCorrectSignatureForSerializationMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerError(DiagnosticId.UseCorrectSignatureForSerializationMethods)
            .WithSpan(7, 22, 7, 29)
            .WithArguments("Handler");

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }
}