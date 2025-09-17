namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferSafeHandleAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.PreferSafeHandle_Title;
    private static readonly LocalizableString MessageFormat = Strings.PreferSafeHandle_Message;
    private static readonly LocalizableString Description = Strings.PreferSafeHandle_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferSafeHandle,
        Title,
        MessageFormat,
        Category.BadPractice,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        var variableDeclaration = fieldDeclaration.Declaration;

        // Check if the field is of type IntPtr or UIntPtr
        var type = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;
        if (type == null || (type.ToString() != "System.IntPtr" && type.ToString() != "System.UIntPtr")
            || fieldDeclaration.Parent is not {} fieldDeclarationParent
            || context.SemanticModel.GetDeclaredSymbol(fieldDeclarationParent) is not INamedTypeSymbol containingType)
        {
            return;
        }

        // Check if the containing type uses the field in unmanaged resource handling
        foreach (var member in containingType.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();

            if (syntaxReference?.GetSyntax(context.CancellationToken) is MethodDeclarationSyntax methodDeclaration && UsesIntPtrOrUIntPtrInUnmanagedContext(context, methodDeclaration, variableDeclaration))
            {
                var diagnostic = Diagnostic.Create(Rule, variableDeclaration.GetLocation(), type.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool UsesIntPtrOrUIntPtrInUnmanagedContext(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration, VariableDeclarationSyntax variableDeclaration)
    {
        if (methodDeclaration.Body is not { } body)
        {
            return false;
        }

        foreach (var statement in body.Statements)
        {
            if (statement is not ExpressionStatementSyntax
                {
                    Expression: InvocationExpressionSyntax invocationExpression
                })
            {
                continue;
            }

            if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol)
            {
                continue;
            }

            foreach (var argument in invocationExpression.ArgumentList.Arguments)
            {
                var argumentType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
                if (argumentType != null && (argumentType.ToString() == "System.IntPtr" || argumentType.ToString() == "System.UIntPtr"))
                {
                    return true;
                }
            }
        }
        return false;
    }
}