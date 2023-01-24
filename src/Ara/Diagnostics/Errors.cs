using Diagnostics;

namespace Ara.Diagnostics;

internal static class Error {
    /// <summary>
    /// CL0003.
    /// </summary>
    internal static Diagnostic UnrecognizedOption(string arg) {
        var message = $"unrecognized command line option '{arg}'";
        return new Diagnostic(ErrorInfo(DiagnosticCode.ERR_UnrecognizedOption), message);
    }

    /// <summary>
    /// CL0004.
    /// </summary>
    internal static Diagnostic MissingFilenameO() {
        var message = "missing filename after '-o'";
        return new Diagnostic(ErrorInfo(DiagnosticCode.ERR_MissingFilenameO), message);
    }

    /// <summary>
    /// CL0005.
    /// </summary>
    internal static Diagnostic CannotSpecifyWithMultipleFiles() {
        var message = "cannot specify output file with '-s' with multiple input files";
        return new Diagnostic(FatalErrorInfo(DiagnosticCode.ERR_CannotSpecifyWithMultipleFiles), message);
    }

    /// <summary>
    /// CL0007.
    /// </summary>
    internal static Diagnostic NoSuchFileOrDirectory(string name) {
        var message = $"{name}: no such file or directory";
        return new Diagnostic(ErrorInfo(DiagnosticCode.ERR_NoSuchFileOrDirectory), message);
    }

    private static DiagnosticInfo ErrorInfo(DiagnosticCode code) {
        return new DiagnosticInfo((int)code, "CL", DiagnosticType.Error);
    }

    private static DiagnosticInfo FatalErrorInfo(DiagnosticCode code) {
        return new DiagnosticInfo((int)code, "CL", DiagnosticType.Fatal);
    }
}
