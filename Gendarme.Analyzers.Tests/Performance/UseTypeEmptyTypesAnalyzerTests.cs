using Gendarme.Analyzers.Performance;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(UseTypeEmptyTypesAnalyzer))]
public sealed class UseTypeEmptyTypesAnalyzerTests
{
    [Fact]
    public async Task TestArrayCreationWithEmptyTypeArray()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        Type[] emptyArray = new Type[0];
    }
}
";

        var context = new CSharpAnalyzerTest<UseTypeEmptyTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.UseTypeEmptyTypes, DiagnosticSeverity.Info)
            .WithSpan(8, 29, 8, 40);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestArrayCreationWithNonEmptyTypeArray()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        Type[] nonEmptyArray = new Type[1];
    }
}
";

        var context = new CSharpAnalyzerTest<UseTypeEmptyTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestArrayCreationWithDifferentElementType()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        int[] emptyArray = new int[0];
    }
}
";

        var context = new CSharpAnalyzerTest<UseTypeEmptyTypesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}