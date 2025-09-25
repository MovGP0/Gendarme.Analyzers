using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(AvoidConstructorsInStaticTypesAnalyzer))]
public sealed class AvoidConstructorsInStaticTypesAnalyzerTests
{
    [Fact]
    public async Task TestStaticClassWithConstructor()
    {
        const string testCode = @"
using System;

public static class MyStaticClass
{
    static MyStaticClass() { }
}
";
        var context = new CSharpAnalyzerTest<AvoidConstructorsInStaticTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidConstructorsInStaticTypes)
            .WithSpan(4, 5, 6, 6)
            .WithArguments("MyStaticClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestStaticClassWithoutConstructor()
    {
        const string testCode = @"
using System;

public static class MyStaticClass
{
    public static void MyMethod() { }
}
";
        var context = new CSharpAnalyzerTest<AvoidConstructorsInStaticTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestNonStaticClassWithStaticMembers()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public static void MyStaticMethod() { }
    
    public MyClass() { } // This is valid
}
";
        var context = new CSharpAnalyzerTest<AvoidConstructorsInStaticTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}