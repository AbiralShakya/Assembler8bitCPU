
namespace Asm;

/// <summary>
/// State of a single assembler.
/// </summary>
public struct AssemblerState {
    /// <summary>
    /// At what point to stop assembly (usually unrestricted).
    /// </summary>
    public AssemblerStage finishStage;

    /// <summary>
    /// The name of the final executable/application.
    /// </summary>
    public string outputFilename;

    /// <summary>
    /// Final file content if stopped after link stage.
    /// </summary>
    public List<byte> linkOutputContent;

    /// <summary>
    /// All files to be managed/modified during assembly.
    /// </summary>
    public FileState[] tasks;
}
