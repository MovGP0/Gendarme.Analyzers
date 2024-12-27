using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(WriteStaticFieldFromInstanceMethodAnalyzer))]
public sealed class WriteStaticFieldFromInstanceMethodAnalyzerTests
{
    [Fact]
    public async Task TestStaticFieldWriteFromInstanceMethod()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private static int myStaticField;

    public void MyInstanceMethod()
    {
        myStaticField = 42; // Writing to a static field from an instance method
    }
}
";

        var context = new CSharpAnalyzerTest<WriteStaticFieldFromInstanceMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.WriteStaticFieldFromInstanceMethod)
            .WithSpan(10, 9, 10, 27)
            .WithArguments("MyInstanceMethod", "myStaticField");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonStaticFieldWriteFromInstanceMethod()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private int myNonStaticField;

    public void MyInstanceMethod()
    {
        myNonStaticField = 42; // Valid write to non-static field
    }
}
";

        var context = new CSharpAnalyzerTest<WriteStaticFieldFromInstanceMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestStaticFieldWriteFromStaticMethod()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private static int myStaticField;

    public static void MyStaticMethod()
    {
        myStaticField = 42; // Valid write to static field from static method
    }
}
";

        var context = new CSharpAnalyzerTest<WriteStaticFieldFromInstanceMethodAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}