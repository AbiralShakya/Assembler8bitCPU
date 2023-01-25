
namespace Asm.Diagnostics;

internal enum DiagnosticCode : int {
    // 0 is reserved for exceptions
    ERR_BadCharacter = 1,
    ERR_UnterminatedString = 2,
    ERR_InvalidType = 3,
}
