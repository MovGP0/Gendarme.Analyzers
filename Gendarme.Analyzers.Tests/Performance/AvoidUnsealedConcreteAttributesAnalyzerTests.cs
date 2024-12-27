using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUnsealedConcreteAttributesAnalyzer))]
public sealed class AvoidUnsealedConcreteAttributesAnalyzerTests
{
    [Fact]
    public async Task TestUnsealedConcreteAttribute()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public class MyAttribute : Attribute { }

public class MyClass : MyAttribute { }
";

        var context = new CSharpAnalyzerTest<AvoidUnsealedConcreteAttributesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidUnsealedConcreteAttributes)
            .WithSpan(5, 14, 5, 25)
            .WithArguments("MyAttribute");

        context.ExpectedDiagnostics.Add(expected1);

        var expected2 = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidUnsealedConcreteAttributes)
            .WithSpan(7, 14, 7, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected2);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestSealedConcreteAttribute()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MySealedAttribute : Attribute { }

[MySealedAttribute]
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<AvoidUnsealedConcreteAttributesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestAbstractAttribute()
    {
        const string testCode = @"
using System;

[AttributeUsage(AttributeTargets.Class)]
public abstract class MyAbstractAttribute : Attribute { }

public class MyClass : MyAbstractAttribute { }
";

        var context = new CSharpAnalyzerTest<AvoidUnsealedConcreteAttributesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var diagnostic = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidUnsealedConcreteAttributes)
            .WithSpan(7, 14, 7, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(diagnostic);
        
        await context.RunAsync();
    }
}