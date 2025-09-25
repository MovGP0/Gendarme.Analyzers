using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(MethodCanBeMadeStaticAnalyzer))]
public sealed class MethodCanBeMadeStaticAnalyzerTests
{
    [Fact]
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

    [Fact]
    public async Task ReportsWhenMethodHasNoInstanceUsage()
    {
        const string source = @"
class C
{
    int M()
    {
        return 42;
    }
}
";

        var expected = new DiagnosticResult(DiagnosticId.MethodCanBeMadeStatic, DiagnosticSeverity.Info)
            .WithSpan(4, 9, 4, 10)
            .WithArguments("M");

        var test = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsWhenMethodUsesProperty()
    {
        const string source = @"
class C
{
    int Age { get; set; }
    int M() => Age;
}
";

        var test = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsWhenMethodUsesInstanceField()
    {
        const string source = @"
class C
{
    int _value;
    int M() => _value;
}
";

        var test = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsWhenMethodCallsInstanceMember()
    {
        const string source = @"
class C
{
    int _value;

    void Helper()
    {
        _value++;
    }

    void M()
    {
        Helper();
    }
}
";

        var test = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsExpressionBodiedMethodUsingInstance()
    {
        const string source = @"
class C
{
    int _value;
    int M() => this._value;
}
";

        var test = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsOverrideMethod()
    {
        const string source = @"
abstract class B
{
    public abstract void M();
}

class C : B
{
    public override void M() { }
}
";

        var test = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task SkipsStaticMethod()
    {
        const string source = @"
class C
{
    static void M() { }
}
";

        var test = new CSharpAnalyzerTest<MethodCanBeMadeStaticAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        await test.RunAsync();
    }
}
