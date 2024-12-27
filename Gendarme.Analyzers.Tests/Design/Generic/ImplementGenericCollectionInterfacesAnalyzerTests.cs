using Gendarme.Analyzers.Design.Generic;

namespace Gendarme.Analyzers.Tests.Design.Generic;

[TestOf(typeof(ImplementGenericCollectionInterfacesAnalyzer))]
public sealed class ImplementGenericCollectionInterfacesAnalyzerTests
{
    [Fact]
    public async Task TestImplementingIEnumerableWarning()
    {
        const string testCode = @"
using System;
using System.Collections;

public class MyClass : IEnumerable
{
    public IEnumerator GetEnumerator() => throw new NotImplementedException();
}
";

        var context = new CSharpAnalyzerTest<ImplementGenericCollectionInterfacesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ImplementGenericCollectionInterfaces)
            .WithSpan(5, 14, 5, 21)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNotImplementingIEnumerableNoWarning()
    {
        const string testCode = @"
using System;
using System.Collections;
using System.Collections.Generic;

public class MyItem
{
}

public class MyClass : IEnumerable<MyItem>
{
    public IEnumerator<MyItem> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
";

        var context = new CSharpAnalyzerTest<ImplementGenericCollectionInterfacesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}