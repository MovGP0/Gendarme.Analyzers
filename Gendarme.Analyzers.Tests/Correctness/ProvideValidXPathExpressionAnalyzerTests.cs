using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ProvideValidXPathExpressionAnalyzer))]
public sealed class ProvideValidXPathExpressionAnalyzerTests
{
    [Fact]
    public async Task Should_ReportWarning_When_InvalidXPathExpression_Used_In_SelectNodes()
    {
        const string testCode = @"
using System.Xml;
using System.Xml.XPath;

public class MyClass
{
    public void MyMethod()
    {
        var someObject = new XmlDocument();
        var result = someObject.SelectNodes(""/books/book["");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXPathExpressionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideValidXPathExpression)
            .WithSpan(10, 45, 10, 59)
            .WithArguments("SelectNodes");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task Should_NotReportWarning_When_ValidXPathExpression_Used_In_SelectNodes()
    {
        const string testCode = @"
using System.Xml;
using System.Xml.XPath;

public class MyClass
{
    public void MyMethod()
    {
        var someObject = new XmlDocument();
        var result = someObject.SelectNodes(""/valid/xpath/expression"");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXPathExpressionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task Should_ReportWarning_When_InvalidXPathExpression_Used_In_Compile()
    {
        const string testCode = @"
using System.Xml.XPath;

public class MyClass
{
    public void MyMethod()
    {
        var expr = XPathExpression.Compile(""/books/book["");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXPathExpressionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideValidXPathExpression)
            .WithSpan(8, 44, 8, 58)
            .WithArguments("Compile");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task Should_NotReportWarning_When_ValidXPathExpression_Used_In_Compile()
    {
        const string testCode = @"
using System.Xml.XPath;

public class MyClass
{
    public void MyMethod()
    {
        var expr = XPathExpression.Compile(""/valid/xpath/expression"");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXPathExpressionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task Should_ReportWarning_When_InvalidConstXPathExpression_Used_In_SelectNodes()
    {
        const string testCode = @"
using System.Xml;

public class MyXmlDocument : XmlDocument
{
    public void MyMethod()
    {
        const string InvalidXPath = ""/books/book["";
        var nodes = SelectNodes(InvalidXPath);
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXPathExpressionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideValidXPathExpression)
            .WithSpan(9, 33, 9, 45)
            .WithArguments("SelectNodes");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task Should_ReportWarning_When_InvalidConstXPathExpression_Used_In_Compile()
    {
        const string testCode = @"
using System.Xml.XPath;

public class MyClass
{
    private const string InvalidXPath = ""/books/book["";

    public void MyMethod()
    {
        var expr = XPathExpression.Compile(InvalidXPath);
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXPathExpressionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideValidXPathExpression)
            .WithSpan(10, 44, 10, 56)
            .WithArguments("Compile");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}
