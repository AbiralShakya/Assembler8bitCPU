using Asm.Diagnostics;
using Diagnostics;

namespace Asm;

/// <summary>
/// Handles assembly and handling a single AssemblerState.
/// Multiple can be created and run asynchronously.
/// </summary>
public sealed class Assembler {
    private const int SUCCESS_EXIT_CODE = 0;
    private const int ERROR_EXIT_CODE = 1;
    private const int FATAL_EXIT_CODE = 2;

    /// <summary>
    /// Creates a new assembler, state needs to be set separately.
    /// </summary>
    public Assembler() {
        diagnostics = new AraDiagnosticQueue();
    }

    /// <summary>
    /// Assembler specific state that determines what to compile and how.
    /// Required to assemble.
    /// </summary>
    public AssemblerState state { get; set; }

    /// <summary>
    /// The name of the assembler (usually displayed with diagnostics).
    /// </summary>
    public string me { get; set; }

    /// <summary>
    /// Where the diagnostics are stored for the compiler before being displayed or logged.
    /// </summary>
    public AraDiagnosticQueue diagnostics { get; set; }

    /// <summary>
    /// Handles preprocessing, assembling, and linking of a set of files.
    /// </summary>
    /// <returns>Error code, 0 = success</returns>
    public int Assemble() {
        int err;

        InternalAssembler();
        err = CheckErrors();
        if (err != SUCCESS_EXIT_CODE)
            return err;

        if (state.finishStage == AssemblerStage.Assembled)
            return SUCCESS_EXIT_CODE;

        InternalLinker();
        err = CheckErrors();
        if (err != SUCCESS_EXIT_CODE)
            return err;

        if (state.finishStage == AssemblerStage.Linked)
            return SUCCESS_EXIT_CODE;

        return FATAL_EXIT_CODE;
    }

    private int CheckErrors() {
        foreach (Diagnostic diagnostic in diagnostics)
            if (diagnostic.info.severity == DiagnosticType.Error)
                return ERROR_EXIT_CODE;

        return SUCCESS_EXIT_CODE;
    }

    private void InternalAssembler() { }

    private void InternalLinker() { }
}
