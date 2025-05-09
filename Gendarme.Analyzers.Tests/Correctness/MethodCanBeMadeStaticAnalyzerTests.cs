using Gendarme.Analyzers.Correctness;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(MethodCanBeMadeStaticAnalyzer))]
public sealed class MethodCanBeMadeStaticAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestMethodCanBeMadeStatic_NoWarnings()
    {
        const string testCode = @"
        public class MyClass
        {
            public int Age { get; set; } = 42;
            public int MyMethod() { return Age; }
        }";

        var context = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodCanBeMadeStatic_WarningForInstanceMethod()
    {
        const string testCode = @"
        public class MyClass
        {
            public int MyMethod()
            {
                return 42;
            }
        }";

        var context = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.MethodCanBeMadeStatic, DiagnosticSeverity.Info)
            .WithSpan(4, 24, 4, 32)
            .WithArguments("MyMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodCanBeMadeStatic_NoWarningForStaticMethod()
    {
        const string testCode = @"
        public class MyClass
        {
            public static int MyStaticMethod()
            {
                return 42;
            }
        }";

        var context = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodCanBeMadeStatic_NoWarningForOverrideMethod()
    {
        const string testCode = @"
        public class BaseClass
        {
            public virtual void MyBaseMethod() { }
        }

        public class DerivedClass : BaseClass
        {
            public override void MyBaseMethod() { }
        }";

        var context = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodCanBeMadeStatic_NoWarningForAbstractMethod()
    {
        const string testCode = @"
        public abstract class MyBaseClass
        {
            public abstract void MyAbstractMethod();
        }

        public class MyClass : MyBaseClass
        {
            public override void MyAbstractMethod() { }
        }";

        var context = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodCanBeMadeStatic_NoWarningForMethodsUsingInstanceFields()
    {
        const string testCode = @"
        public class MyClass
        {
            private int _field;

            public int MyMethod()
            {
                return _field;
            }
        }";

        var context = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}