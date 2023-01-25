using Asm.CodeAnalysis.Symbols;
using Asm.CodeAnalysis.Text;
using Diagnostics;

namespace Asm.Diagnostics;

internal static class Error {
    /// <summary>
    /// AS0001.
    /// </summary>
    internal static AraDiagnostic BadCharacter(TextLocation location, int position, char input) {
        var message = $"unknown character '{input}'";
        return new AraDiagnostic(ErrorInfo(DiagnosticCode.ERR_BadCharacter), location, message);
    }

    /// <summary>
    /// AS0002.
    /// </summary>
    internal static AraDiagnostic UnterminatedString(TextLocation location) {
        var message = "unterminated string literal";
        return new AraDiagnostic(ErrorInfo(DiagnosticCode.ERR_UnterminatedString), location, message);
    }

    /// <summary>
    /// AS0003.
    /// </summary>
    internal static AraDiagnostic InvalidType(TextLocation location, string text, TypeSymbol type) {
        var message = $"'{text}' is not a valid '{type}'";
        return new AraDiagnostic(ErrorInfo(DiagnosticCode.ERR_InvalidType), location, message);
    }


    private static DiagnosticInfo ErrorInfo(DiagnosticCode code) {
        return new DiagnosticInfo((int)code, "BU", DiagnosticType.Error);
    }

    private static DiagnosticInfo FatalErrorInfo(DiagnosticCode code) {
        return new DiagnosticInfo((int)code, "BU", DiagnosticType.Fatal);
    }
}
