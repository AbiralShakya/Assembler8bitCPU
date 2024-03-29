using System.Collections.Immutable;

namespace Asm.CodeAnalysis.Syntax;

internal static partial class SyntaxFactory {
    internal static SyntaxToken Token(SyntaxTree syntaxTree, SyntaxKind kind, int position, string text) {
        return new SyntaxToken(
            syntaxTree, kind, position, text, null,
            ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty
        );
    }

    internal static SyntaxToken Token(SyntaxKind kind, string text) {
        return Token(null, kind, -1, text);
    }

    internal static SyntaxToken Token(SyntaxTree syntaxTree, SyntaxKind kind, int position) {
        return Token(syntaxTree, kind, position, null);
    }

    // internal static SyntaxToken Identifier(string name) {
    //     return new SyntaxToken(null, SyntaxKind.IdentifierToken, -1, name, null,
    //         ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty
    //     );
    // }

    // internal static NameExpressionSyntax Name(string name) {
    //     return new NameExpressionSyntax(null, Token(SyntaxKind.IdentifierToken, name));
    // }
}
