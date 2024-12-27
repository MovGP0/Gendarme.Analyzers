using Gendarme.Analyzers.Design.Generic;

namespace Gendarme.Analyzers.Tests.Design.Generic;

[TestOf(typeof(DoNotExposeNestedGenericSignaturesAnalyzer))]
public sealed class DoNotExposeNestedGenericSignaturesAnalyzerTests
{
    [Fact]
    public async Task TestAvoidNestedGenericReturnType()
    {
        const string testCode = @"
using System.Collections.Generic;

public class OuterClass
{
    public List<InnerClass<T>> GetInnerClass<T>()
    {
        return new List<InnerClass<T>>();
    }

    public class InnerClass<U>{}
}
";

        var context = new CSharpAnalyzerTest<DoNotExposeNestedGenericSignaturesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotExposeNestedGenericSignatures)
            .WithSpan(6, 32, 6, 45)
            .WithArguments("GetInnerClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAvoidNestedGenericParameterType()
    {
        const string testCode = @"
using System.Collections.Generic;

public class OuterClass
{
    public void MethodWithGeneric<T>(List<InnerClass<T>> parameter)
    {
    }

    public class InnerClass<U>{}
}
";

        var context = new CSharpAnalyzerTest<DoNotExposeNestedGenericSignaturesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotExposeNestedGenericSignatures)
            .WithSpan(6, 17, 6, 34)
            .WithArguments("MethodWithGeneric");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}