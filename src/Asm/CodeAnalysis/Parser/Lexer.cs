using System;
using System.Collections.Immutable;
using System.Text;
using Asm.CodeAnalysis.Symbols;
using Asm.CodeAnalysis.Text;
using Asm.Diagnostics;

namespace Asm.CodeAnalysis.Syntax.InternalSyntax;

/// <summary>
/// Converts source text into parsable SyntaxTokens.<br/>
/// E.g.
/// <code>
/// mov 2 a
/// --->
/// MovToken NumberToken IdentifierToken
/// </code>
/// </summary>
internal sealed class Lexer {
    internal char current => Peek(0);
    internal char lookahead => Peek(1);

    private readonly SourceText _text;
    private int _position;
    private int _start;
    private SyntaxKind _kind;
    private object _value;
    private SyntaxTree _syntaxTree;
    private ImmutableArray<SyntaxTrivia>.Builder _triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();

    /// <summary>
    /// Creates a new <see cref="Lexer" />, requires a fully initialized <see cref="SyntaxTree" />.
    /// </summary>
    /// <param name="syntaxTree"><see cref="SyntaxTree" /> to lex from.</param>
    internal Lexer(SyntaxTree syntaxTree) {
        _text = syntaxTree.text;
        _syntaxTree = syntaxTree;
        diagnostics = new AraDiagnosticQueue();
    }

    /// <summary>
    /// Diagnostics produced during lexing process
    /// </summary>
    internal AraDiagnosticQueue diagnostics { get; set; }

    /// <summary>
    /// Lexes the next un-lexed text to create a single <see cref="SyntaxToken" />.
    /// </summary>
    /// <returns>A new <see cref="SyntaxToken" />.</returns>
    internal SyntaxToken LexNext() {
        ReadTrivia(true);
        var leadingTrivia = _triviaBuilder.ToImmutable();
        var tokenStart = _position;

        ReadToken();

        var tokenKind = _kind;
        var tokenValue = _value;
        var tokenLength = _position - _start;

        ReadTrivia(false);
        var trailingTrivia = _triviaBuilder.ToImmutable();

        var tokenText = SyntaxFacts.GetText(tokenKind);
        if (tokenText == null)
            tokenText = _text.ToString(tokenStart, tokenLength);

        return new SyntaxToken(
            _syntaxTree, tokenKind, tokenStart, tokenText, tokenValue, leadingTrivia, trailingTrivia
        );
    }

    private char Peek(int offset) {
        int index = _position + offset;

        if (index >= _text.length)
            return '\0';

        return _text[index];
    }

    private void ReadTrivia(bool leading) {
        _triviaBuilder.Clear();
        var done = false;

        while (!done) {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            switch (current) {
                case '\0':
                    done = true;
                    break;
                case ';':
                    ReadSingeLineComment();
                    break;
                case '\r':
                case '\n':
                    if (!leading)
                        done = true;
                    ReadLineBreak();
                    break;
                case ' ':
                case '\t':
                    ReadWhitespace();
                    break;
                default:
                    if (char.IsWhiteSpace(current))
                        ReadWhitespace();
                    else
                        done = true;
                    break;
            }

            var length = _position - _start;

            if (length > 0) {
                var text = _text.ToString(_start, length);
                var trivia = new SyntaxTrivia(_syntaxTree, _kind, _start, text);
                _triviaBuilder.Add(trivia);
            }
        }
    }

    private void ReadToken() {
        _start = _position;
        _kind = SyntaxKind.BadToken;
        _value = null;

        switch (current) {
            case '\0':
                _kind = SyntaxKind.EndOfFileToken;
                break;
            case '"':
                ReadStringLiteral();
                break;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                ReadNumericLiteral();
                break;
            case '_':
                ReadIdentifierOrKeyword();
                break;
            default:
                if (char.IsLetter(current))
                    ReadIdentifierOrKeyword();
                else {
                    var span = new TextSpan(_position, 1);
                    var location = new TextLocation(_text, span);
                    diagnostics.Push(Error.BadCharacter(location, _position, current));
                    _position++;
                }

                break;
        }
    }

    private void ReadSingeLineComment() {
        _position += 2;
        var done = false;

        while (!done) {
            switch (current) {
                case '\r':
                case '\n':
                case '\0':
                    done = true;
                    break;
                default:
                    _position++;
                    break;
            }
        }

        _kind = SyntaxKind.SingleLineCommentTrivia;
    }

    private void ReadStringLiteral() {
        _position++;
        var sb = new StringBuilder();
        bool done = false;

        while (!done) {
            switch (current) {
                case '\0':
                case '\r':
                case '\n':
                    var span = new TextSpan(_start, 1);
                    var location = new TextLocation(_text, span);
                    diagnostics.Push(Error.UnterminatedString(location));
                    done = true;
                    break;
                case '"':
                    if (lookahead == '"') {
                        sb.Append(current);
                        _position += 2;
                    } else {
                        _position++;
                        done = true;
                    }
                    break;
                case '\\':
                    _position++;

                    switch (current) {
                        case 'a':
                            sb.Append('\a');
                            _position++;
                            break;
                        case 'b':
                            sb.Append('\b');
                            _position++;
                            break;
                        case 'f':
                            sb.Append('\f');
                            _position++;
                            break;
                        case 'n':
                            sb.Append('\n');
                            _position++;
                            break;
                        case 'r':
                            sb.Append('\r');
                            _position++;
                            break;
                        case 't':
                            sb.Append('\t');
                            _position++;
                            break;
                        case 'v':
                            sb.Append('\v');
                            _position++;
                            break;
                        case '\'':
                            sb.Append('\'');
                            _position++;
                            break;
                        case '"':
                            sb.Append('"');
                            _position++;
                            break;
                        case '\\':
                            sb.Append('\\');
                            _position++;
                            break;
                        default:
                            sb.Append('\\');
                            break;
                    }
                    break;
                default:
                    sb.Append(current);
                    _position++;
                    break;
            }
        }

        _kind = SyntaxKind.StringLiteralToken;
        _value = sb.ToString();
    }

    private void ReadNumericLiteral() {
        var isBinary = false;
        var isHexadecimal = false;
        char? previous = null;

        bool isValidCharacter(char c) {
            if (isBinary && c == '0' || c == '1') {
                return true;
            } else if (isHexadecimal && char.IsAsciiHexDigit(c)) {
                return true;
            } else if (!isBinary && !isHexadecimal && char.IsDigit(c)) {
                return true;
            } else {
                return false;
            }
        }

        if (current == '0') {
            if (char.ToLower(lookahead) == 'b') {
                isBinary = true;
                _position += 2;
            } else if (char.ToLower(lookahead) == 'x') {
                isHexadecimal = true;
                _position += 2;
            }
        }

        while (true) {
            if (current == '_' && previous.HasValue && isValidCharacter(lookahead)) {
                _position++;
            } else if (isValidCharacter(current)) {
                _position++;
            } else {
                break;
            }

            previous = Peek(-1);
        }

        int length = _position - _start;
        string text = _text.ToString(_start, length);
        string parsedText = text.Replace("_", "");

        var @base = isBinary ? 2 : 16;
        var failed = false;
        byte value = 0;

        if (isBinary || isHexadecimal) {
            try {
                value = Convert.ToByte(
                    text.Length > 2 ? parsedText.Substring(2) : throw new FormatException(), @base);
            } catch (Exception e) when (e is OverflowException || e is FormatException) {
                failed = true;
            }
        } else if (!byte.TryParse(parsedText, out value)) {
            failed = true;
        }

        if (failed) {
            var span = new TextSpan(_start, length);
            var location = new TextLocation(_text, span);
            diagnostics.Push(Error.InvalidType(location, text, TypeSymbol.Byte));
        } else {
            _value = value;
        }

        _kind = SyntaxKind.NumericLiteralToken;
    }

    private void ReadWhitespace() {
        var done = false;

        while (!done) {
            switch (current) {
                case '\0':
                case '\r':
                case '\n':
                    done = true;
                    break;
                default:
                    if (!char.IsWhiteSpace(current))
                        done = true;
                    else
                        _position++;
                    break;
            }
        }

        _kind = SyntaxKind.WhitespaceTrivia;
    }

    private void ReadLineBreak() {
        if (current == '\r' && lookahead == '\n')
            _position += 2;
        else
            _position++;

        _kind = SyntaxKind.EndOfLineTrivia;
    }

    private void ReadIdentifierOrKeyword() {
        while (char.IsLetterOrDigit(current) || current == '_')
            _position++;

        int length = _position - _start;
        string text = _text.ToString(_start, length);
        _kind = SyntaxFacts.GetKeywordType(text);
    }
}
