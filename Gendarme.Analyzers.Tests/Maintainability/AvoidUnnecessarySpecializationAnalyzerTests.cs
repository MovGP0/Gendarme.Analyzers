using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidUnnecessarySpecializationAnalyzer))]
public sealed class AvoidUnnecessarySpecializationAnalyzerTests
{
    [Fact]
    public async Task Suggest_Interface_When_Concrete_Type_Is_OverSpecialized()
    {
        // Matches the analyzer summary: suggest using the most general interface
        const string testCode = @"public interface IHasher
{
int GetHashCode(object o);
}
public class DefaultHasher : IHasher
{
public int GetHashCode(object o) => o.GetHashCode();
public int Custom() => 42;
}
public class MyClass
{
public int Bad(DefaultHasher ec, object o)
{
return ec.GetHashCode(o);
}
}";

        var context = new CSharpAnalyzerTest<AvoidUnnecessarySpecializationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidUnnecessarySpecialization)
            .WithSpan(12, 16, 12, 29)
            .WithArguments("ec", "IHasher");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task No_Diagnostic_When_Interface_Is_Already_Used()
    {
        const string testCode = @"public interface IMovable
{
void Move();
}
public class Car : IMovable
{
public void Move() {}
public void Drive() {}
}
public class MyClass
{
public void Good(IMovable m)
{
m.Move();
}
}";

        var context = new CSharpAnalyzerTest<AvoidUnnecessarySpecializationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task No_Diagnostic_When_Concrete_Only_Members_Are_Required()
    {
        const string testCode = @"public interface IMovable
{
void Move();
}
public class Car : IMovable
{
public void Move() {}
public void Drive() {}
}
public class MyClass
{
public void Method(Car car)
{
car.Drive(); // uses a member not on IMovable -> cannot generalize
}
}";

        var context = new CSharpAnalyzerTest<AvoidUnnecessarySpecializationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task No_Diagnostic_When_Parameter_Is_Unused()
    {
        const string testCode = @"public interface IFoo { void A(); }
public class Foo : IFoo { public void A() {} public void B() {} }
public class MyClass
{
public void Method(Foo f) { /* f is not used */ }
}";

        var context = new CSharpAnalyzerTest<AvoidUnnecessarySpecializationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}