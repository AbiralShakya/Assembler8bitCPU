using Asm.CodeAnalysis.Text;

namespace Asm.CodeAnalysis.Syntax;

/// <summary>
/// All trivia: comments and whitespace. Text that does not affect assembly.
/// </summary>
internal sealed class SyntaxTrivia {
    /// <param name="position">Position of the trivia (indexed by nodes, not by character).</param>
    /// <param name="text">Text associated with the trivia.</param>
    internal SyntaxTrivia(SyntaxTree syntaxTree, SyntaxKind kind, int position, string text) {
        this.syntaxTree = syntaxTree;
        this.position = position;
        this.kind = kind;
        this.text = text;
    }

    internal SyntaxTree syntaxTree { get; }

    internal SyntaxKind kind { get; }

    /// <summary>
    /// The position of the <see cref="SyntaxTrivia" />.
    /// </summary>
    internal int position { get; }

    /// <summary>
    /// <see cref="TextSpan" /> of where the <see cref="SyntaxTrivia" /> is in the <see cref="SourceText" />.
    /// </summary>
    internal TextSpan span => new TextSpan(position, text?.Length ?? 0);

    /// <summary>
    /// Text associated with the <see cref="SyntaxTrivia" />.
    /// </summary>
    internal string text { get; }
}
