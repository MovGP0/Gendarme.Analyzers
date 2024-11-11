namespace Gendarme.Analyzers.Extensions;

public static class StatementSyntaxExtensions
{
    public static StatementSyntax? GetPreviousStatement(this StatementSyntax statement)
    {
        var parent = statement.Parent;
        if (parent is BlockSyntax block)
        {
            var statements = block.Statements;
            var index = statements.IndexOf(statement);
            if (index > 0)
            {
                return statements[index - 1];
            }
        }
        return null;
    }
}