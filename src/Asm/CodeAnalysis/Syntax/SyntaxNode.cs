using System.Collections.Generic;
using System.Linq;
using Asm.CodeAnalysis.Text;

namespace Asm.CodeAnalysis.Syntax;

/// <summary>
/// Base building block of all things.
/// Because of generators, the order of fields in a <see cref="SyntaxNode" /> child class need to correctly reflect the
/// source file.
/// </summary>
internal abstract class SyntaxNode {
    protected SyntaxNode(SyntaxTree syntaxTree) {
        this.syntaxTree = syntaxTree;
    }

    /// <summary>
    /// Type of <see cref="SyntaxNode" /> (see <see cref="SyntaxKind" />).
    /// </summary>
    internal abstract SyntaxKind kind { get; }

    /// <summary>
    /// <see cref="SyntaxTree" /> this <see cref="SyntaxNode" /> resides in.
    /// </summary>
    internal SyntaxTree syntaxTree { get; }

    /// <summary>
    /// <see cref="TextSpan" /> of where the <see cref="SyntaxNode" /> is in the <see cref="SourceText" />
    /// (not including line break).
    /// </summary>
    internal virtual TextSpan span {
        get {
            var children = GetChildren();

            if (children.ToArray().Length == 0)
                return null;

            var first = children.First().span;
            var last = children.Last().span;

            if (first == null || last == null)
                return null;

            return TextSpan.FromBounds(first.start, last.end);
        }
    }

    /// <summary>
    /// <see cref="TextSpan" /> of where the <see cref="SyntaxNode" /> is in the <see cref="SourceText" />
    /// (including line break).
    /// </summary>
    internal virtual TextSpan fullSpan {
        get {
            var children = GetChildren();

            if (children.ToArray().Length == 0)
                return null;

            var first = children.First().fullSpan;
            var last = children.Last().fullSpan;

            if (first == null || last == null)
                return null;

            return TextSpan.FromBounds(first.start, last.end);
        }
    }

    /// <summary>
    /// Location of where the <see cref="SyntaxNode" /> is in the <see cref="SourceText" />.
    /// </summary>
    internal TextLocation location => syntaxTree == null ? null : new TextLocation(syntaxTree.text, span);

    /// <summary>
    /// Gets all child SyntaxNodes.
    /// Order should be consistent of how they look in a file, but calling code should not depend on that.
    /// </summary>
    internal abstract IEnumerable<SyntaxNode> GetChildren();

    /// <summary>
    /// Gets last <see cref="SyntaxToken" /> (of all children, recursive) under this <see cref="SyntaxNode" />.
    /// </summary>
    /// <returns>Last <see cref="SyntaxToken" />.</returns>
    internal SyntaxToken GetLastToken() {
        if (this is SyntaxToken t)
            return t;

        return GetChildren().Last().GetLastToken();
    }
}
