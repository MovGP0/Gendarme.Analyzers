using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(ImplementEqualsTypeAnalyzer))]
public sealed class ImplementEqualsTypeAnalyzerTests
{
    [Fact]
    public async Task TestClassWithoutIEquatable()
    {
        const string testCode = @"
public class MyClass
{
    public override bool Equals(object obj) => base.Equals(obj);
}";

        var context = new CSharpAnalyzerTest<ImplementEqualsTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementEqualsType)
            .WithSpan(3, 14, 3, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassWithIEquatableAndEquals()
    {
        const string testCode = @"
using System;

public class MyClass : IEquatable<MyClass>
{
    public override bool Equals(object obj) => base.Equals(obj);

    public bool Equals(MyClass other)
    {
        return other != null;
    }
}";

        var context = new CSharpAnalyzerTest<ImplementEqualsTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestClassWithOnlyIEquatable()
    {
        const string testCode = @"
using System;

public class MyClass : IEquatable<MyClass>
{
    public bool Equals(MyClass other)
    {
        return other != null;
    }
}";

        var context = new CSharpAnalyzerTest<ImplementEqualsTypeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementEqualsType)
            .WithSpan(3, 14, 3, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}