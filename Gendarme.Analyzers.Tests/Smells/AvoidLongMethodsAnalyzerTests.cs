using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidLongMethodsAnalyzer))]
public sealed class AvoidLongMethodsAnalyzerTests
{
    [Fact]
    public async Task TestMethodWithTooManyStatements()
    {
        const string testCode = @"
public class MyClass
{
    public void LongMethod()
    {
        // 41 statements (this is just an example, each line could represent a statement)
        var a = 1; var b = 2; var c = 3; var d = 4; var e = 5;
        var f = 6; var g = 7; var h = 8; var i = 9; var j = 10;
        var k = 11; var l = 12; var m = 13; var n = 14; var o = 15;
        var p = 16; var q = 17; var r = 18; var s = 19; var t = 20;
        var u = 21; var v = 22; var w = 23; var x = 24; var y = 25;
        var z = 26; var aa = 27; var ab = 28; var ac = 29; var ad = 30;
        var ae = 31; var af = 32; var ag = 33; var ah = 34; var ai = 35;
        var aj = 36; var ak = 37; var al = 38; var am = 39; var an = 40;
        var ao = 41;
    }
}";

        var context = new CSharpAnalyzerTest<AvoidLongMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidLongMethods)
            .WithSpan(4, 17, 4, 27)
            .WithArguments("LongMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodWithAcceptableLength()
    {
        const string testCode = @"
public class MyClass
{
    public void ShortMethod()
    {
        var a = 1;
        var b = 2;
    }
}";

        var context = new CSharpAnalyzerTest<AvoidLongMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestWellKnownMethodIsIgnored()
    {
        const string testCode = @"
public class Form
{
    public void InitializeComponent() 
    {
        // some statements
        var a = 1;
        var b = 2;
        var c = 3;
    }
}";

        var context = new CSharpAnalyzerTest<AvoidLongMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for well-known method
        await context.RunAsync();
    }
}