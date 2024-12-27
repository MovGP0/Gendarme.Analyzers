using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoubleCheckLockingAnalyzer))]
public sealed class DoubleCheckLockingAnalyzerTests
{
    [Fact]
    public async Task TestDoubleCheckLocking()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private static object _lock = new object();
    private static MyClass _instance;

    public static MyClass Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new MyClass();
                    }
                }
            }
            return _instance;
        }
    }
}";

        var context = new CSharpAnalyzerTest<DoubleCheckLockingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoubleCheckLocking)
            .WithSpan(13, 13, 22, 14)
            .WithArguments("_lock");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestNoDoubleCheckLocking()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private static object _lock = new object();
    private static MyClass _instance;

    public static MyClass Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new MyClass();
                }
            }
            return _instance;
        }
    }
}";

        var context = new CSharpAnalyzerTest<DoubleCheckLockingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}