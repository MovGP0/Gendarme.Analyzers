using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidComplexMethodsAnalyzer))]
public sealed class AvoidComplexMethodsAnalyzerTests
{
    [Fact]
    public async Task TestMethodComplexityExceedsThreshold()
    {
        // 26 if-statements -> CC = 1 + 26 = 27 (> 25)
        const string testCode = @"public class MyClass
{
public void ComplexMethod()
{
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {}
}
}";

        var context = new CSharpAnalyzerTest<AvoidComplexMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidComplexMethods, DiagnosticSeverity.Info)
            .WithSpan(3, 13, 3, 26)
            .WithArguments("ComplexMethod", 27);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodComplexityAtOrBelowThreshold_NoDiagnostic()
    {
        // 24 if-statements -> CC = 25 (== 25) => no diagnostic
        const string testCode = @"public class MyClass
{
public void SimpleMethod()
{
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {} if (true) {} if (true) {}
}
}";

        var context = new CSharpAnalyzerTest<AvoidComplexMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected (CC == 25)
        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestConstructorComplexityExceedsThreshold()
    {
        // 26 if-statements in constructor -> CC = 27 (>25)
        const string testCode = @"public class MyClass
{
public MyClass()
{
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {} if (true) {} if (true) {} if (true) {} if (true) {}
if (true) {} if (true) {}
}
}";

        var context = new CSharpAnalyzerTest<AvoidComplexMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidComplexMethods, DiagnosticSeverity.Info)
            .WithSpan(3, 8, 3, 15)
            .WithArguments("MyClass", 27);
        
        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestConstructorComplexityBelowThreshold_NoDiagnostic()
    {
        // 1 if-statement -> CC = 2 (<=25) => no diagnostic
        const string testCode = @"public class MyClass
{
public MyClass()
{
if (true) {}
}
}";

        var context = new CSharpAnalyzerTest<AvoidComplexMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestSwitchBasedComplexity_ExceedsThreshold()
    {
        // switch with 26 case labels -> CC = 1 + 26 = 27 (>25)
        const string testCode = @"public class MyClass
{
public void ComplexSwitch()
{
int x = 0;
switch (x) {
case 0: break; case 1: break; case 2: break; case 3: break; case 4: break; case 5: break;
case 6: break; case 7: break; case 8: break; case 9: break; case 10: break; case 11: break;
case 12: break; case 13: break; case 14: break; case 15: break; case 16: break; case 17: break;
case 18: break; case 19: break; case 20: break; case 21: break; case 22: break; case 23: break;
case 24: break; case 25: break;
}
}
}";

        var context = new CSharpAnalyzerTest<AvoidComplexMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidComplexMethods, DiagnosticSeverity.Info)
            .WithSpan(3, 13, 3, 26)
            .WithArguments("ComplexSwitch", 27);
        
        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}