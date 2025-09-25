using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidLargeClassesAnalyzer))]
public sealed class AvoidLargeClassesAnalyzerTests
{
    [Fact]
    public async Task TestLargeClassWithTooManyFields()
    {
        const string testCode = @"
public class MyLargeClass
{
    private int field1;
    private int field2;
    private int field3;
    private int field4;
    private int field5;
    private int field6;
    private int field7;
    private int field8;
    private int field9;
    private int field10;
    private int field11;
    private int field12;
    private int field13;
    private int field14;
    private int field15;
    private int field16;
    private int field17;
    private int field18;
    private int field19;
    private int field20;
    private int field21;
    private int field22;
    private int field23;
    private int field24;
    private int field25;
    private int field26; // This field makes it exceed the limit
}
";

        var context = new CSharpAnalyzerTest<AvoidLargeClassesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidLargeClasses)
            .WithSpan(2, 14, 2, 26)
            .WithArguments("MyLargeClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassWithFieldPrefixes()
    {
        const string testCode = @"
public class MyClass
{
    private int myField1;
    private int myField2;
    private int myField3;
    private int myField4;
    private int myField5;
    private int myField6;
    private int otherField1;
    private int otherField2;
}
";

        var context = new CSharpAnalyzerTest<AvoidLargeClassesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult.CompilerWarning(DiagnosticId.AvoidLargeClasses)
            .WithSpan(2, 14, 2, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidClass()
    {
        const string testCode = @"
public class ValidClass
{
    private int field1;
    private int field2;
}
";

        var context = new CSharpAnalyzerTest<AvoidLargeClassesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}