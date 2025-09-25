using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidUnnecessarySpecializationAnalyzer))]
public sealed class AvoidUnnecessarySpecializationAnalyzerTests
{
    [Fact]
    public async Task TestUnnecessarySpecializationDetection()
    {
        const string testCode = @"
public class Base { }
public class Derived : Base { }

public class MyClass
{
    public void Method(Derived parameter) { }
}
";

        var context = new CSharpAnalyzerTest<AvoidUnnecessarySpecializationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidUnnecessarySpecialization)
            .WithSpan(6, 26, 6, 33)
            .WithArguments("parameter", "Base");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoUnnecessarySpecialization()
    {
        const string testCode = @"
public class Base { }
public class MyClass
{
    public void Method(Base parameter) { }
}
";

        var context = new CSharpAnalyzerTest<AvoidUnnecessarySpecializationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}