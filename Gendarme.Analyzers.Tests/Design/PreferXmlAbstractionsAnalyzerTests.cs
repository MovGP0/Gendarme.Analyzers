using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(PreferXmlAbstractionsAnalyzer))]
public sealed class PreferXmlAbstractionsAnalyzerTests
{
    [Fact]
    public async Task TestPublicMethodWithXmlDocumentParameter()
    {
        const string testCode = @"
using System.Xml;
public class MyXmlClass
{
    public void ProcessXml(XmlDocument doc) { }
}
";

        var context = new CSharpAnalyzerTest<PreferXmlAbstractionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.PreferXmlAbstractions, DiagnosticSeverity.Info)
            .WithSpan(5, 17, 5, 27)
            .WithArguments("ProcessXml", "XmlDocument");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPublicPropertyWithXmlNodeType()
    {
        const string testCode = @"
using System.Xml;
public class MyXmlNodeClass
{
    public XmlNode MyNode { get; set; }
}
";

        var context = new CSharpAnalyzerTest<PreferXmlAbstractionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.PreferXmlAbstractions, DiagnosticSeverity.Info)
            .WithSpan(5, 20, 5, 26)
            .WithArguments("MyNode", "XmlNode");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestProtectedMethodWithXmlElementReturnType()
    {
        const string testCode = @"
using System.Xml;
public class MyXmlElementClass
{
    protected XmlElement CreateElement() 
    {
        return null; 
    }
}
";

        var context = new CSharpAnalyzerTest<PreferXmlAbstractionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.PreferXmlAbstractions, DiagnosticSeverity.Info)
            .WithSpan(5, 26, 5, 39)
            .WithArguments("CreateElement", "XmlElement");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoDiagnosticsForInternalAccess()
    {
        const string testCode = @"
using System.Xml;
class MyInternalXmlClass
{
    public void ProcessXml(XmlDocument doc) { }
}
";

        var context = new CSharpAnalyzerTest<PreferXmlAbstractionsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}