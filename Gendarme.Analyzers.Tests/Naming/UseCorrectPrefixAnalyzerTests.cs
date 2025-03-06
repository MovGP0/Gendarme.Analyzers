using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(UseCorrectPrefixAnalyzer))]
public sealed class UseCorrectPrefixAnalyzerTests
{
    [Fact]
    public async Task TestInterfaceNamingConvention()
    {
        const string testCode = @"
            public interface MyInterface { }
        ";

        var context = new CSharpAnalyzerTest<UseCorrectPrefixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectPrefix)
            .WithSpan(2, 30, 2, 41)
            .WithArguments("Interface", "MyInterface", "should be prefixed with 'I'");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvalidClassNamePrefix()
    {
        const string testCode = @"
            public class CMyClass { }
        ";

        var context = new CSharpAnalyzerTest<UseCorrectPrefixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectPrefix)
            .WithSpan(2, 26, 2, 34)
            .WithArguments("Type", "CMyClass", "should not be prefixed with 'C'");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidGenericParameterName()
    {
        const string testCode = @"
            public class MyClass<T> { }
        ";

        var context = new CSharpAnalyzerTest<UseCorrectPrefixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvalidGenericParameterName()
    {
        const string testCode = @"
            public class MyClass<Xyz> { }
        ";

        var context = new CSharpAnalyzerTest<UseCorrectPrefixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectPrefix)
            .WithSpan(2, 15, 2, 30)
            .WithArguments("Generic parameter", "Xyz", "should be a single uppercase letter or prefixed with 'T'");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}