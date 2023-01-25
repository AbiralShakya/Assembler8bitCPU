using System.Collections.Generic;
using System.Collections.Immutable;
using Asm.CodeAnalysis.Text;
using Asm.Diagnostics;
using static Asm.CodeAnalysis.Syntax.SyntaxFactory;

namespace Asm.CodeAnalysis.Syntax.InternalSyntax;

/// <summary>
/// Lexes then parses text into a tree of SyntaxNodes, in doing so doing syntax checking.
/// </summary>
internal sealed class Parser {
    private readonly ImmutableArray<SyntaxToken> _tokens;
    private readonly SourceText _text;
    private readonly SyntaxTree _syntaxTree;
    private int _position;

    /// <summary>
    /// Creates a new <see cref="Parser" />, requiring a fully initialized <see cref="SyntaxTree" />.
    /// </summary>
    /// <param name="syntaxTree"><see cref="SyntaxTree" /> to parse from.</param>
    internal Parser(SyntaxTree syntaxTree) {
        diagnostics = new AraDiagnosticQueue();
        var tokens = new List<SyntaxToken>();
        var badTokens = new List<SyntaxToken>();
        var lexer = new Lexer(syntaxTree);
        SyntaxToken token;
        _text = syntaxTree.text;
        _syntaxTree = syntaxTree;

        do {
            token = lexer.LexNext();

            if (token.kind == SyntaxKind.BadToken) {
                badTokens.Add(token);
                continue;
            }

            if (badTokens.Count > 0) {
                var leadingTrivia = token.leadingTrivia.ToBuilder();
                var index = 0;

                foreach (var badToken in badTokens) {
                    foreach (var lt in badToken.leadingTrivia)
                        leadingTrivia.Insert(index++, lt);

                    var trivia = new SyntaxTrivia(
                        syntaxTree, SyntaxKind.SkippedTokenTrivia, badToken.position, badToken.text
                    );

                    leadingTrivia.Insert(index++, trivia);

                    foreach (var tt in badToken.trailingTrivia)
                        leadingTrivia.Insert(index++, tt);
                }

                badTokens.Clear();
                token = new SyntaxToken(token.syntaxTree, token.kind, token.position,
                    token.text, token.value, leadingTrivia.ToImmutable(), token.trailingTrivia
                );
            }

            tokens.Add(token);
        } while (token.kind != SyntaxKind.EndOfFileToken);

        _tokens = tokens.ToImmutableArray();
        diagnostics.Move(lexer.diagnostics);
    }

    /// <summary>
    /// Diagnostics produced during the parsing process.
    /// </summary>
    internal AraDiagnosticQueue diagnostics { get; set; }

    private SyntaxToken current => Peek(0);

    /// <summary>
    /// Parses the entirety of a single file.
    /// </summary>
    /// <returns>The parsed file.</returns>
    internal CompilationUnitSyntax ParseCompilationUnit() {
        var members = ParseMembers();
        var endOfFile = Match(SyntaxKind.EndOfFileToken);

        return new CompilationUnitSyntax(_syntaxTree, members, endOfFile);
    }

    private SyntaxToken Match(SyntaxKind kind, SyntaxKind? nextWanted = null) {
        if (current.kind == kind)
            return Next();

        if (nextWanted != null && current.kind == nextWanted) {
            diagnostics.Push(Error.ExpectedToken(current.location, kind));

            return Token(_syntaxTree, kind, current.position);
        }

        if (Peek(1).kind != kind) {
            diagnostics.Push(Error.UnexpectedToken(current.location, current.kind, kind));
            SyntaxToken skipped = current;
            _position++;

            return Token(_syntaxTree, kind, skipped.position);
        }

        diagnostics.Push(Error.UnexpectedToken(current.location, current.kind));
        _position++;
        SyntaxToken saved = current;
        _position++;

        return saved;
    }

    private SyntaxToken Next() {
        SyntaxToken saved = current;
        _position++;

        return saved;
    }

    private SyntaxToken Peek(int offset) {
        var index = _position + offset;

        if (index >= _tokens.Length)
            return _tokens[_tokens.Length - 1];

        if (index < 0)
            return _tokens[0];

        return _tokens[index];
    }

    private ImmutableArray<MemberSyntax> ParseMembers() {
        var members = ImmutableArray.CreateBuilder<MemberSyntax>();

        while (current.kind != SyntaxKind.EndOfFileToken) {
            var startToken = current;

            var member = ParseMember();
            members.Add(member);

            if (current == startToken)
                Next();
        }

        return members.ToImmutable();
    }

    private MemberSyntax ParseMember() {
        switch (current.kind) {
            default:
                return ParseGlobalStatement();
        }
    }

    private MemberSyntax ParseGlobalStatement() {
        // var statement = ParseStatement();
        StatementSyntax statement = null;

        return new GlobalStatementSyntax(_syntaxTree, statement);
    }
}
