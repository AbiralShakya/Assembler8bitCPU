
namespace Ara.Diagnostics;

internal enum DiagnosticCode : int {
    WRN_CorruptInstallation = 1,
    WRN_IgnoringAssembledFile = 2,
    ERR_UnrecognizedOption = 3,
    ERR_MissingFilenameO = 4,
    ERR_CannotSpecifyWithMultipleFiles = 5,
    WRN_IgnoringUnknownFileType = 6,
    ERR_NoSuchFileOrDirectory = 7,
}
