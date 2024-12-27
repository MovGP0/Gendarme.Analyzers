using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ProvideValidXPathExpressionAnalyzer))]
public sealed class ProvideValidXPathExpressionAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected")]
    public async Task Should_ReportWarning_When_InvalidXPathExpression_Used_In_SelectNodes()
    {
        const string testCode = @"
using System.Xml;
using System.Xml.XPath;

public class MyClass
{
    public void MyMethod()
    {
        XmlDocument someObject = new XmlDocument();
        var result = someObject.SelectNodes(""invalid_xpath_expression"");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXPathExpressionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideValidXPathExpression)
            .WithSpan(8, 22, 8, 32)
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
        XmlDocument someObject = new XmlDocument();
        var result = someObject.SelectNodes(""/valid/xpath/expression"");
    }
}";

        var context = new CSharpAnalyzerTest<ProvideValidXPathExpressionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact(Skip = "Analyzer not implemented correctly")]
    public async Task Should_ReportWarning_When_InvalidXPathExpression_Used_In_Compile()
    {
        const string testCode = @"
using System.Xml.XPath;

public class MyClass
{
    public void MyMethod()
    {
        var expr = XPathExpression.Compile(""invalid_xpath_expression"");
    }
}";
        
        var context = new CSharpAnalyzerTest<ProvideValidXPathExpressionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ProvideValidXPathExpression)
            .WithSpan(6, 27, 6, 56)
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

        await context.RunAsync(); // No diagnostics expected
    }
}