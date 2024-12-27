using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(PreferXmlAbstractionsAnalyzer))]
public sealed class PreferXmlAbstractionsAnalyzerTests
{
    [Fact]
    public async Task TestPublicMethodWithXmlDocumentParameter()
    {
        const string testCode = @"
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

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferXmlAbstractions)
            .WithSpan(4, 18, 4, 31)
            .WithArguments("ProcessXml", "XmlDocument");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPublicPropertyWithXmlNodeType()
    {
        const string testCode = @"
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

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferXmlAbstractions)
            .WithSpan(4, 16, 4, 25)
            .WithArguments("MyNode", "XmlNode");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestProtectedMethodWithXmlElementReturnType()
    {
        const string testCode = @"
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

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferXmlAbstractions)
            .WithSpan(4, 17, 4, 31)
            .WithArguments("CreateElement", "XmlElement");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoDiagnosticsForInternalAccess()
    {
        const string testCode = @"
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