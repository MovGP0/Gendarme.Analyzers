using Gendarme.Analyzers.Design.Generic;

namespace Gendarme.Analyzers.Tests.Design.Generic;

[TestOf(typeof(UseGenericEventHandlerAnalyzer))]
public sealed class UseGenericEventHandlerAnalyzerTests
{
    [Fact]
    public async Task TestUseGenericEventHandlerWarning()
    {
        const string testCode = @"
        using System;

        public delegate void MyDelegate(object sender, EventArgs e);

        public class MyClass
        {
            public event MyDelegate MyEvent;
        }";

        var context = new CSharpAnalyzerTest<UseGenericEventHandlerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseGenericEventHandler)
            .WithSpan(4, 9, 4, 69);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestWarningForNonGenericEventHandler()
    {
        const string testCode = @"
        using System;

        public delegate void MyGenericDelegate<TEventArgs>(object sender, TEventArgs e);

        public class MyClass
        {
            public event MyGenericDelegate<EventArgs> MyEvent;
        }";

        var context = new CSharpAnalyzerTest<UseGenericEventHandlerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var diagnostic = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseGenericEventHandler)
            .WithSpan(4, 9, 4, 89);

        context.ExpectedDiagnostics.Add(diagnostic);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoWarningForGenericEventHandler()
    {
        const string testCode = @"
        using System;

        public class MyClass
        {
            public event EventHandler<EventArgs>? MyEvent;
        }";

        var context = new CSharpAnalyzerTest<UseGenericEventHandlerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestNoWarningForEventHandler()
    {
        const string testCode = @"
        using System;

        public class MyClass
        {
            public event EventHandler? MyEvent;
        }";

        var context = new CSharpAnalyzerTest<UseGenericEventHandlerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}