
namespace Asm;

/// <summary>
/// The current step in assembly a source file is.
/// </summary>
public enum AssemblerStage {
    Raw,
    Assembled,
    Linked,
}
