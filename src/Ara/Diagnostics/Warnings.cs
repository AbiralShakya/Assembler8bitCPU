using Diagnostics;

namespace Ara.Diagnostics;

internal static class Warning {
    /// <summary>
    /// CL0001.
    /// </summary>
    internal static Diagnostic CorruptInstallation() {
        var message =
            $"installation is corrupt; all assembler features are enabled except the `--help` option";
        return new Diagnostic(WarningInfo(DiagnosticCode.WRN_CorruptInstallation), message);
    }

    /// <summary>
    /// CL0002.
    /// </summary>
    internal static Diagnostic IgnoringAssembledFile(string filename) {
        var message = $"{filename}: file already assembled; ignoring";
        return new Diagnostic(WarningInfo(DiagnosticCode.WRN_IgnoringAssembledFile), message);
    }

    /// <summary>
    /// CL0006.
    /// </summary>
    internal static Diagnostic IgnoringUnknownFileType(string filename) {
        var message = $"unknown file type of input file '{filename}'; ignoring";
        return new Diagnostic(WarningInfo(DiagnosticCode.WRN_IgnoringUnknownFileType), message);
    }

    private static DiagnosticInfo WarningInfo(DiagnosticCode code) {
        return new DiagnosticInfo((int)code, "CL", DiagnosticType.Warning);
    }
}
