using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewInconsistentIdentityAnalyzer))]
public sealed class ReviewInconsistentIdentityAnalyzerTests
{
    [Fact]
    public async Task EqualsCompareToAndGetHashCode_UseSameFields_NoDiagnostic()
    {
        const string testCode = @"
using System;

public class MyClass : IComparable<MyClass>, ICloneable
{
    private readonly int a;
    private readonly int b;

    public MyClass(int a, int b) { this.a = a; this.b = b; }

    public override bool Equals(object obj)
        => obj is MyClass other && a == other.a && b == other.b;

    public override int GetHashCode()
        => HashCode.Combine(a, b);

    public int CompareTo(MyClass other)
        => a != other.a ? a.CompareTo(other.a) : b.CompareTo(other.b);

    public static bool operator ==(MyClass x, MyClass y) => x.Equals(y);
    public static bool operator !=(MyClass x, MyClass y) => !x.Equals(y);

    public object Clone() => new MyClass(a, b);
}
";

        var test = new CSharpAnalyzerTest<ReviewInconsistentIdentityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task GetHashCode_UsesSubsetOfEquals_NoDiagnostic()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private int a, b;

    public override bool Equals(object obj)
    {
        return obj is MyClass other && a == other.a && b == other.b;
    }

    public override int GetHashCode()
    {
        return a; // subset of fields used by Equals
    }
}
";

        var test = new CSharpAnalyzerTest<ReviewInconsistentIdentityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task CompareTo_UsesDifferentFieldsThanEquals_Diagnostic()
    {
        const string testCode = @"
using System;

public class MyClass : IComparable<MyClass>
{
    private int a, b;

    public override bool Equals(object obj)
        => obj is MyClass other && a == other.a; // uses 'a'

    public int CompareTo(MyClass other)
        => b.CompareTo(other.b); // uses 'b'
}
";

        var test = new CSharpAnalyzerTest<ReviewInconsistentIdentityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult.CompilerWarning(DiagnosticId.ReviewInconsistentIdentity)
            .WithArguments("MyClass");

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task GetHashCode_NotSubsetOfEquals_Diagnostic()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private int a, b;

    public override bool Equals(object obj)
        => obj is MyClass other && a == other.a; // uses 'a'

    public override int GetHashCode()
        => b; // uses 'b' only -> not a subset of Equals
}
";

        var test = new CSharpAnalyzerTest<ReviewInconsistentIdentityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult.CompilerWarning(DiagnosticId.ReviewInconsistentIdentity)
            .WithArguments("MyClass");

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task Clone_SupersetOfEquals_NoDiagnostic()
    {
        const string testCode = @"
using System;

public class MyClass : ICloneable
{
    private int a, b;

    public override bool Equals(object obj)
        => obj is MyClass other && a == other.a; // uses 'a'

    public object Clone()
        => new MyClass { a = this.a, b = this.b }; // superset of fields used by Equals
}
";

        var test = new CSharpAnalyzerTest<ReviewInconsistentIdentityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await test.RunAsync();
    }

    [Fact]
    public async Task Clone_NotSupersetOfEquals_Diagnostic()
    {
        const string testCode = @"
using System;

public class MyClass : ICloneable
{
    private int a, b;

    public override bool Equals(object obj)
        => obj is MyClass other && a == other.a && b == other.b; // uses 'a' and 'b'

    public object Clone()
        => new MyClass { a = this.a }; // misses 'b' -> not superset
}
";

        var test = new CSharpAnalyzerTest<ReviewInconsistentIdentityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult.CompilerWarning(DiagnosticId.ReviewInconsistentIdentity)
            .WithArguments("MyClass");

        test.ExpectedDiagnostics.Add(expected);
        await test.RunAsync();
    }

    [Fact]
    public async Task HelperMethods_UsedAcrossEquality_NoDiagnostic()
    {
        const string testCode = @"
using System;

public class MyClass : IComparable<MyClass>, ICloneable
{
    private int a, b;

    public override bool Equals(object obj)
        => obj is MyClass other && EqualsCore(other);

    private bool EqualsCore(MyClass other) => a == other.a && b == other.b;

    public override int GetHashCode() => HashCore();
    private int HashCore() => HashCode.Combine(a, b);

    public int CompareTo(MyClass other) => CompareCore(other);
    private int CompareCore(MyClass other) => a != other.a ? a.CompareTo(other.a) : b.CompareTo(other.b);

    public object Clone() => CloneCore();
    private MyClass CloneCore() => new MyClass { a = this.a, b = this.b };
}
";

        var test = new CSharpAnalyzerTest<ReviewInconsistentIdentityAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await test.RunAsync();
    }
}