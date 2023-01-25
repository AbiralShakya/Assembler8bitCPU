
namespace Asm;

/// <summary>
/// The state of a source file.
/// </summary>
public struct FileState {
    /// <summary>
    /// Original name of source file.
    /// </summary>
    public string inputFilename;

    /// <summary>
    /// Current stage of the file (see AssemblerStage).
    /// Not related to the stage of the assembler as a whole.
    /// </summary>
    public AssemblerStage stage;

    /// <summary>
    /// Name of the file that the new contents will be put into (if applicable).
    /// </summary>
    public string outputFilename;

    /// <summary>
    /// The content of the file (not just of the original file).
    /// </summary>
    public FileContent fileContent;
}
