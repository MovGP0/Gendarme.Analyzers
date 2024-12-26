using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(DoNotUseGetInterfaceToCheckAssignabilityAnalyzer))]
public sealed class DoNotUseGetInterfaceToCheckAssignabilityAnalyzerTests
{
    [Fact]
    public async Task TestGetInterfaceUsage()
    {
        const string testCode = @"
using System;

public interface IMyInterface {}
public class MyClass : IMyInterface {}

public class Test
{
    public void Method()
    {
        if (typeof(MyClass).GetInterface(""IMyInterface"") != null) {}
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotUseGetInterfaceToCheckAssignabilityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotUseGetInterfaceToCheckAssignability)
            .WithSpan(11, 13, 11, 57)
            .WithArguments("IMyInterface");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCorrectAssignabilityCheck()
    {
        const string testCode = @"
using System;

public interface IMyInterface {}
public class MyClass : IMyInterface {}

public class Test
{
    public void Method()
    {
        if (!typeof(IMyInterface).IsAssignableFrom(typeof(MyClass))) {}
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotUseGetInterfaceToCheckAssignabilityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected, as the code correctly uses IsAssignableFrom.
        await context.RunAsync();
    }
}