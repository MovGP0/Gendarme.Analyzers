using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotRecurseInEqualityAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotRecurseInEquality_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotRecurseInEquality_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotRecurseInEquality_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotRecurseInEquality,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeOperator, SyntaxKind.OperatorDeclaration);
    }

    private static void AnalyzeOperator(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not OperatorDeclarationSyntax declaration)
        {
            return;
        }

        var token = declaration.OperatorToken;

        if (!token.IsKind(SyntaxKind.EqualsEqualsToken) && !token.IsKind(SyntaxKind.ExclamationEqualsToken))
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not IMethodSymbol operatorSymbol)
        {
            return;
        }

        var walker = new RecursiveEqualityWalker(context.SemanticModel, operatorSymbol, context.CancellationToken);

        if (declaration.Body is { } body)
        {
            walker.Visit(body);
        }
        else if (declaration.ExpressionBody is { Expression: { } expression })
        {
            walker.Visit(expression);
        }

        if (walker.FoundRecursiveCall)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, token.GetLocation()));
        }
    }

    private sealed class RecursiveEqualityWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel semanticModel;
        private readonly IMethodSymbol operatorSymbol;
        private readonly System.Threading.CancellationToken cancellationToken;

        internal bool FoundRecursiveCall { get; private set; }

        internal RecursiveEqualityWalker(SemanticModel semanticModel, IMethodSymbol operatorSymbol, System.Threading.CancellationToken cancellationToken)
            : base(SyntaxWalkerDepth.Node)
        {
            this.semanticModel = semanticModel;
            this.operatorSymbol = operatorSymbol;
            this.cancellationToken = cancellationToken;
        }

        public override void Visit(SyntaxNode? node)
        {
            if (node is null || FoundRecursiveCall)
            {
                return;
            }

            base.Visit(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (FoundRecursiveCall)
            {
                return;
            }

            if (node.IsKind(SyntaxKind.EqualsExpression) || node.IsKind(SyntaxKind.NotEqualsExpression))
            {
                if (semanticModel.GetOperation(node, cancellationToken) is IBinaryOperation { OperatorMethod: { } method }
                    && IsRecursiveOperator(method))
                {
                    FoundRecursiveCall = true;
                    return;
                }
            }

            base.VisitBinaryExpression(node);
        }

        private bool IsRecursiveOperator(IMethodSymbol method)
        {
            if (SymbolEqualityComparer.Default.Equals(method, operatorSymbol))
            {
                return true;
            }

            return SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, operatorSymbol);
        }
    }
}
