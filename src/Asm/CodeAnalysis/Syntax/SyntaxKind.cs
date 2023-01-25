
namespace Asm.CodeAnalysis.Syntax;

/// <summary>
/// All types of things to be found in a source file.
/// </summary>
internal enum SyntaxKind {
    // Tokens with text
    BadToken,
    IdentifierToken,
    StringLiteralToken,
    NumericLiteralToken,

    // Trivia
    EndOfLineTrivia,
    WhitespaceTrivia,
    SingleLineCommentTrivia,
    SkippedTokenTrivia,

    // Other
    EndOfFileToken,
    CompilationUnit,
    GlobalStatement,
}
