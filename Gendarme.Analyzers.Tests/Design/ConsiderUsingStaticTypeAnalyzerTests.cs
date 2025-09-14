using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ConsiderUsingStaticTypeAnalyzer))]
public sealed class ConsiderUsingStaticTypeAnalyzerTests
{
    [Fact]
    public async Task TestClassWithAllStaticMembers()
    {
        const string testCode = @"
public class MyClass
{
    public static void MyStaticMethod() { }
    public static int MyStaticProperty { get; set; }
}
";

        var context = new CSharpAnalyzerTest<ConsiderUsingStaticTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.ConsiderUsingStaticType, DiagnosticSeverity.Info)
            .WithSpan(2, 14, 2, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassWithInstanceMembers()
    {
        const string testCode = @"
public class MyClass
{
    public void MyInstanceMethod() { }
    public int MyInstanceProperty { get; set; }
}
";

        var context = new CSharpAnalyzerTest<ConsiderUsingStaticTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassAlreadyStatic()
    {
        const string testCode = @"
public static class MyStaticClass
{
    public static void MyStaticMethod() { }
}
";

        var context = new CSharpAnalyzerTest<ConsiderUsingStaticTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassWithoutMembers()
    {
        const string testCode = @"
public class MyEmptyClass { }
";

        var context = new CSharpAnalyzerTest<ConsiderUsingStaticTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.ConsiderUsingStaticType, DiagnosticSeverity.Info)
            .WithSpan(2, 14, 2, 26)
            .WithArguments("MyEmptyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}