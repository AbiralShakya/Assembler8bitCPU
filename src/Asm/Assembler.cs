using Ara.Diagnostics;
using Asm.Diagnostics;
using Diagnostics;

namespace Asm;

/// <summary>
/// The current step in assembly a source file is.
/// </summary>
public enum AssemblerStage {
    Raw,
    Preprocessed,
    Assembled,
    Linked,
}

/// <summary>
/// Contents of a file either represented as text or bytes.
/// </summary>
public struct FileContent {
    /// <summary>
    /// Text representation of file
    /// </summary>
    public string text;

    /// <summary>
    /// Byte representation of file (usually only used with .o or .exe files)
    /// </summary>
    public List<byte> bytes;
}

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

/// <summary>
/// A type of compilation that will be performed, only one per compilation.
/// </summary>
public enum BuildMode {
    Independent,
}

/// <summary>
/// State of a single assembler.
/// </summary>
public struct AssemblerState {
    /// <summary>
    /// What the assembler will target.
    /// </summary>
    public BuildMode buildMode;

    /// <summary>
    /// The name of the final executable/application (if applicable).
    /// </summary>
    public string moduleName;

    /// <summary>
    /// Assembler time options (see AraCommandLine)
    /// </summary>
    public string[] options;

    /// <summary>
    /// Where the application will start.
    /// </summary>
    public string entryPoint;

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

        InternalPreprocessor();

        err = CheckErrors();
        if (err != SUCCESS_EXIT_CODE)
            return err;

        if (state.finishStage == AssemblerStage.Preprocessed)
            return SUCCESS_EXIT_CODE;

        diagnostics.Push(Error.Unsupported.IndependentCompilation());

        return CheckErrors();

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

    private void InternalAssembler() {
        diagnostics.Push(Warning.Unsupported.Assembling());
    }

    private void InternalLinker() {
        diagnostics.Push(Warning.Unsupported.Linking());
    }

    private void InternalPreprocessor() {
        var preprocessor = new Preprocessor();

        for (int i=0; i<state.tasks.Length; i++) {
            ref FileState task = ref state.tasks[i];

            if (task.stage == AssemblerStage.Raw)
                task.stage = AssemblerStage.Preprocessed;

            var text = preprocessor.PreprocessText(task.inputFilename, task.fileContent.text);
            task.fileContent.text = text;
        }
    }

    private void InternalAssembler() { }

    private void InternalLinker() { }
}
