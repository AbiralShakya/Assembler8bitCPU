
namespace Asm.CodeAnalysis.Syntax;

/// <summary>
/// A statement <see cref="SyntaxNode" />, a line of code that is its own idea.
/// Statements end with a line break.
/// </summary>
internal abstract class StatementSyntax : SyntaxNode {
    protected StatementSyntax(SyntaxTree syntaxTree): base(syntaxTree) { }
}
