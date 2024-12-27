using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(OperatorEqualsShouldBeOverloadedAnalyzer))]
public sealed class OperatorEqualsShouldBeOverloadedAnalyzerTests
{
    [Fact]
    public async Task TestValueTypeWithoutOperatorEquals()
    {
        const string testCode = @"
public struct MyStruct
{
    public int Value;

    public override bool Equals(object obj)
    {
        return obj is MyStruct other && Value == other.Value;
    }
}
";

        var context = new CSharpAnalyzerTest<OperatorEqualsShouldBeOverloadedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.OperatorEqualsShouldBeOverloaded)
            .WithSpan(3, 1, 6, 1)
            .WithArguments("MyStruct");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValueTypeWithOperatorEquals()
    {
        const string testCode = @"
public struct MyStruct
{
    public int Value;

    public override bool Equals(object obj)
    {
        return obj is MyStruct other && Value == other.Value;
    }

    public static bool operator ==(MyStruct a, MyStruct b) => a.Equals(b);
    public static bool operator !=(MyStruct a, MyStruct b) => !a.Equals(b);
}
";

        var context = new CSharpAnalyzerTest<OperatorEqualsShouldBeOverloadedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestClassWithOperatorEqualsButNoOverride()
    {
        const string testCode = @"
public class MyClass
{
    public int Value;

    public static bool operator ==(MyClass a, MyClass b) => a.Value == b.Value;
    public static bool operator !=(MyClass a, MyClass b) => a.Value != b.Value;
}
";

        var context = new CSharpAnalyzerTest<OperatorEqualsShouldBeOverloadedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestStructWithOperatorsAddAndSubWithoutOperatorEquals()
    {
        const string testCode = @"
public struct MyStruct
{
    public int Value;

    public static MyStruct operator +(MyStruct a, MyStruct b) => new MyStruct { Value = a.Value + b.Value };
    public static MyStruct operator -(MyStruct a, MyStruct b) => new MyStruct { Value = a.Value - b.Value };
}
";

        var context = new CSharpAnalyzerTest<OperatorEqualsShouldBeOverloadedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.OperatorEqualsShouldBeOverloaded)
            .WithSpan(3, 1, 8, 1)
            .WithArguments("MyStruct");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}