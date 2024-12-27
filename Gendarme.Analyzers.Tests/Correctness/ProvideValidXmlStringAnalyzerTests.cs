using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ProvideValidXmlStringAnalyzer))]
public sealed class ProvideValidXmlStringAnalyzerTests
{
    [Fact]
    public async Task TestValidXmlString()
    {
        const string testCode = @"
using System.Xml;

public class MyClass
{
    public void Method()
    {
        var doc = new XmlDocument();
        doc.LoadXml(""<root></root>"");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXmlStringAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        context.ExpectedDiagnostics.Clear(); // No issues expected

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvalidXmlString()
    {
        const string testCode = @"
using System.Xml;

public class MyClass
{
    public void Method()
    {
        var doc = new XmlDocument();
        doc.LoadXml(""<root><invalid></root>"");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXmlStringAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideValidXmlString)
            .WithSpan(9, 21, 9, 45)
            .WithArguments("LoadXml");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}