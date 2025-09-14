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
            .WithSpan(2, 18, 2, 29)  // Span covers "MyInterface" (exclusive end)
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
            .WithSpan(2, 14, 2, 22)  // Start after "public class " and cover "CMyClass" (exclusive end)
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
public class Container<wrong>  // incorrect name for type parameter
{
}";

        var context = new CSharpAnalyzerTest<UseCorrectPrefixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectPrefix)
            .WithSpan(2, 24, 2, 29)  // Just the type parameter name "wrong" (exclusive end)
            .WithArguments("Generic parameter", "wrong", "should be a single uppercase letter or prefixed with 'T'");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidGenericParameterNames()
    {
        const string testCode = @"
public class Container<T>  // Single uppercase letter
{
    public T Value { get; set; }
}

public class Pair<TFirst, TSecond>  // T-prefixed names
{
    public TFirst First { get; set; }
    public TSecond Second { get; set; }
}";

        var context = new CSharpAnalyzerTest<UseCorrectPrefixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestEdgeCaseGenericParameterNames()
    {
        const string testCode = @"
public class EdgeCases<
    t,              // invalid: lowercase single letter
    T1,             // invalid: T followed by number
    Tfoo,           // invalid: T followed by lowercase
    TFoo,           // valid: T followed by uppercase
    K,              // valid: single uppercase letter
    k>              // invalid: lowercase single letter
{
}";

        var context = new CSharpAnalyzerTest<UseCorrectPrefixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectPrefix)
            .WithSpan(3, 5, 3, 6)   // 't' (exclusive end)
            .WithArguments("Generic parameter", "t", "should be a single uppercase letter or prefixed with 'T'");

        var expected2 = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectPrefix)
            .WithSpan(4, 5, 4, 7)   // 'T1' (exclusive end)
            .WithArguments("Generic parameter", "T1", "should be a single uppercase letter or prefixed with 'T'");

        var expected3 = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectPrefix)
            .WithSpan(5, 5, 5, 9)   // 'Tfoo' (exclusive end)
            .WithArguments("Generic parameter", "Tfoo", "should be a single uppercase letter or prefixed with 'T'");

        var expected4 = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectPrefix)
            .WithSpan(8, 5, 8, 6)   // 'k' (exclusive end)
            .WithArguments("Generic parameter", "k", "should be a single uppercase letter or prefixed with 'T'");

        context.ExpectedDiagnostics.Add(expected1);
        context.ExpectedDiagnostics.Add(expected2);
        context.ExpectedDiagnostics.Add(expected3);
        context.ExpectedDiagnostics.Add(expected4);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestComplexGenericParameterNames()
    {
        const string testCode = @"
public class Complex<
    TMyEntityType,      // valid: T + PascalCase
    TFooBarBaz,        // valid: T + PascalCase
    TResult1,          // invalid: contains number
    TFOO,             // valid: T + uppercase
    TFOOBar,          // valid: T + mixed case
    TXml>             // valid: T + common abbreviation
{
}";

        var context = new CSharpAnalyzerTest<UseCorrectPrefixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseCorrectPrefix)
            .WithSpan(5, 5, 5, 13)   // 'TResult1' (exclusive end)
            .WithArguments("Generic parameter", "TResult1", "should be a single uppercase letter or prefixed with 'T'");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}
