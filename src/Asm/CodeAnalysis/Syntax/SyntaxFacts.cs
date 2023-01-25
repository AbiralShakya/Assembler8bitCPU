using System;
using System.Collections.Generic;
using Asm.Diagnostics;

namespace Asm.CodeAnalysis.Syntax;

/// <summary>
/// Basic syntax facts references by the <see cref="Parser" /> and the <see cref="Lexer" />.
/// </summary>
internal static class SyntaxFacts {
    /// <summary>
    /// Gets binary operator precedence of a <see cref="SyntaxKind" />.
    /// </summary>
    /// <param name="type"><see cref="SyntaxKind" />.</param>
    /// <returns>Precedence, or 0 if <paramref name="type" /> is not a binary operator.</returns>
    internal static int GetBinaryPrecedence(this SyntaxKind type) {
        switch (type) {
            default:
                return 0;
        }
    }

    /// <summary>
    /// Gets unary operator precedence of a <see cref="SyntaxKind" />.
    /// </summary>
    /// <param name="type"><see cref="SyntaxKind" />.</param>
    /// <returns>Precedence, or 0 if <paramref name="type" /> is not a unary operator.</returns>
    internal static int GetUnaryPrecedence(this SyntaxKind type) {
        switch (type) {
            default:
                return 0;
        }
    }

    /// <summary>
    /// Attempts to get a <see cref="SyntaxKind" /> from a text representation of a keyword.
    /// </summary>
    /// <param name="text">Text representation.</param>
    /// <returns>Keyword kind, defaults to identifer if failed.</returns>
    internal static SyntaxKind GetKeywordType(string text) {
        switch (text) {
            default:
                return SyntaxKind.IdentifierToken;
        }
    }

    /// <summary>
    /// Gets text representation of a <see cref="SyntaxToken" /> or keyword.
    /// </summary>
    /// <param name="type"><see cref="SyntaxKind" />.</param>
    /// <returns>Text representation, default to null if not text representation exists.</returns>
    internal static string GetText(SyntaxKind type) {
        switch (type) {
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets all unary operator types.
    /// </summary>
    /// <returns>Unary operator types (calling code should not depend on order).</returns>
    internal static IEnumerable<SyntaxKind> GetUnaryOperatorTypes() {
        var types = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var type in types) {
            if (GetUnaryPrecedence(type) > 0)
                yield return type;
        }
    }

    /// <summary>
    /// Gets all binary operator types.
    /// </summary>
    /// <returns>Binary operator types (calling code should not depend on order).</returns>
    internal static IEnumerable<SyntaxKind> GetBinaryOperatorTypes() {
        var types = (SyntaxKind[])Enum.GetValues(typeof(SyntaxKind));
        foreach (var type in types) {
            if (GetBinaryPrecedence(type) > 0)
                yield return type;
        }
    }

    /// <summary>
    /// Checks if a <see cref="SyntaxKind" /> is a keyword.
    /// </summary>
    /// <param name="type"><see cref="SyntaxKind" />.</param>
    /// <returns>If the <see cref="SyntaxKind" /> is a keyword.</returns>
    internal static bool IsKeyword(this SyntaxKind type) {
        return type.ToString().EndsWith("Keyword");
    }

    /// <summary>
    /// Checks if a <see cref="SyntaxKind" /> is a <see cref="SyntaxToken" />.
    /// </summary>
    /// <param name="type"><see cref="SyntaxKind" />.</param>
    /// <returns>If the <see cref="SyntaxKind" /> is a token.</returns>
    internal static bool IsToken(this SyntaxKind type) {
        return !type.IsTrivia() && (type.IsKeyword() || type.ToString().EndsWith("Token"));
    }

    /// <summary>
    /// Checks if a <see cref="SyntaxKind" /> is trivia.
    /// </summary>
    /// <param name="type"><see cref="SyntaxKind" />.</param>
    /// <returns>If the <see cref="SyntaxKind" /> is trivia.</returns>
    internal static bool IsTrivia(this SyntaxKind type) {
        return type.ToString().EndsWith("Trivia");
    }

    /// <summary>
    /// Checks if a <see cref="SyntaxKind" /> is a comment.
    /// </summary>
    /// <param name="type"><see cref="SyntaxKind" />.</param>
    /// <returns>If the <see cref="SyntaxKind" /> is a comment.</returns>
    internal static bool IsComment(this SyntaxKind type) {
        return type == SyntaxKind.SingleLineCommentTrivia;
    }
}
