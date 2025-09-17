using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseObjectDisposedExceptionAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseObjectDisposedExceptionTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseObjectDisposedExceptionMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseObjectDisposedExceptionDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseObjectDisposedException,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
        if (!typeSymbol.Interfaces.Any(i => i.ToString() == "System.IDisposable"))
            return;

        var disposeField = typeSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f.Name == "disposed" && f.Type.SpecialType == SpecialType.System_Boolean);
        if (disposeField == null)
            return;

        foreach (var method in typeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (method is not { MethodKind: MethodKind.Ordinary } || method.Name == "Dispose")
                continue;

            var syntaxReference = method.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference is null)
                continue;

            if (syntaxReference.GetSyntax(context.CancellationToken) is not MethodDeclarationSyntax methodDeclaration)
                continue;

            if (!MethodUsesDisposedFlag(methodDeclaration, disposeField.Name))
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    methodDeclaration.Identifier.GetLocation(),
                    method.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool MethodUsesDisposedFlag(MethodDeclarationSyntax methodDeclaration, string disposedFieldName)
    {
        if (methodDeclaration.ExpressionBody is ArrowExpressionClauseSyntax expressionBody)
        {
            return ExpressionUsesDisposedFlag(expressionBody.Expression, disposedFieldName);
        }

        if (methodDeclaration.Body is null)
        {
            return false;
        }

        return methodDeclaration.Body.DescendantNodes()
            .Any(node => node is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == disposedFieldName);
    }

    private static bool ExpressionUsesDisposedFlag(ExpressionSyntax expression, string disposedFieldName)
    {
        return expression.DescendantNodesAndSelf()
            .Any(node => node is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == disposedFieldName);
    }
}