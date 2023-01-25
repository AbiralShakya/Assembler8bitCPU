using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Asm;
using Asm.CodeAnalysis.Text;
using Asm.Diagnostics;
using Diagnostics;

namespace Ara.CommandLine;

/// <summary>
/// Handles all command-line interaction, argument parsing, and <see cref="Assembler" /> invocation.
/// </summary>
public static partial class AraCommandLine {
    private const int SuccessExitCode = 0;
    private const int ErrorExitCode = 1;
    private const int FatalExitCode = 2;

    /// <summary>
    /// Processes/decodes command-line arguments, and invokes <see cref="Assembler" />.
    /// </summary>
    /// <param name="args">Command-line arguments from Main.</param>
    /// <returns>Error code, 0 = success.</returns>
    public static int ProcessArgs(string[] args) {
        int err;
        var assembler = new Assembler();
        assembler.me = Process.GetCurrentProcess().ProcessName;

        assembler.state = DecodeOptions(
            args, out DiagnosticQueue<Diagnostic> diagnostics, out ShowDialogs dialogs
        );

        var hasDialog = dialogs.version || dialogs.help;

        var resources = Path.Combine(GetExecutingPath(), "Resources");
        var corrupt = false;

        if (!Directory.Exists(resources)) {
            corrupt = true;
            ResolveDiagnostic(Ara.Diagnostics.Warning.CorruptInstallation(), assembler.me);
        }

        if (hasDialog)
            diagnostics.Clear();

        if (dialogs.version)
            ShowVersionDialog();

        if (dialogs.help && !corrupt)
            ShowHelpDialog();

        if (hasDialog)
            return SuccessExitCode;

        err = ResolveDiagnostics(diagnostics, assembler.me);

        if (err > 0)
            return err;

        ResolveOutputFiles(assembler);
        ReadInputFiles(assembler, out diagnostics);

        err = ResolveDiagnostics(diagnostics, assembler.me);

        if (err > 0)
            return err;

        assembler.Assemble();

        err = ResolveDiagnostics(assembler);

        if (err > 0)
            return err;

        ResolveAssemblerOutput(assembler);

        return SuccessExitCode;
    }

    private static void ShowHelpDialog() {
        var path = Path.Combine(GetExecutingPath(), "Resources/HelpPrompt.txt");
        var helpMessage = File.ReadAllText(path);
        Console.WriteLine(helpMessage);
    }

    private static string GetExecutingPath() {
        var executingLocation = Assembly.GetExecutingAssembly().Location;
        var executingPath = System.IO.Path.GetDirectoryName(executingLocation);

        return executingPath;
    }

    private static void ShowVersionDialog() {
        var versionMessage = "Version: Ara 0.1";
        Console.WriteLine(versionMessage);
    }

    private static void PrettyPrintDiagnostic(AraDiagnostic diagnostic, ConsoleColor? textColor) {
        void ResetColor() {
            if (textColor != null)
                Console.ForegroundColor = textColor.Value;
            else
                Console.ResetColor();
        }

        var span = diagnostic.location.span;
        var text = diagnostic.location.text;

        var lineNumber = text.GetLineIndex(span.start);
        var line = text.lines[lineNumber];
        var column = span.start - line.start + 1;
        var lineText = line.ToString();

        var filename = diagnostic.location.fileName;

        if (!string.IsNullOrEmpty(filename))
            Console.Write($"{filename}:");

        Console.Write($"{lineNumber + 1}:{column}:");

        var highlightColor = ConsoleColor.White;

        var severity = diagnostic.info.severity;

        if (severity == DiagnosticType.Error) {
            highlightColor = ConsoleColor.Red;
            Console.ForegroundColor = highlightColor;
            Console.Write(" error");
        } else if (severity == DiagnosticType.Fatal) {
            highlightColor = ConsoleColor.Red;
            Console.ForegroundColor = highlightColor;
            Console.Write(" fatal error");
        } else if (severity == DiagnosticType.Warning) {
            highlightColor = ConsoleColor.Magenta;
            Console.ForegroundColor = highlightColor;
            Console.Write(" warning");
        }

        if (diagnostic.info.code != null && diagnostic.info.code > 0) {
            var number = diagnostic.info.code.ToString();
            Console.Write($" AS{number.PadLeft(4, '0')}: ");
        } else {
            Console.Write(": ");
        }

        ResetColor();
        Console.WriteLine(diagnostic.message);

        if (text.IsAtEndOfInput(span))
            return;

        var prefixSpan = TextSpan.FromBounds(line.start, span.start);
        var suffixSpan = TextSpan.FromBounds(span.end, line.end);

        var prefix = text.ToString(prefixSpan);
        var focus = text.ToString(span);
        var suffix = text.ToString(suffixSpan);

        Console.Write($" {prefix}");
        Console.ForegroundColor = highlightColor;
        Console.Write(focus);
        ResetColor();
        Console.WriteLine(suffix);

        Console.ForegroundColor = highlightColor;
        var markerPrefix = " " + Regex.Replace(prefix, @"\S", " ");
        var marker = "^";

        if (span.length > 0 && column != lineText.Length)
            marker += new string('~', span.length - 1);

        Console.WriteLine(markerPrefix + marker);

        if (diagnostic.suggestion != null) {
            Console.ForegroundColor = ConsoleColor.Green;
            var suggestion = diagnostic.suggestion.Replace("%", focus);
            Console.WriteLine(markerPrefix + suggestion);
        }

        ResetColor();
    }

    private static DiagnosticType ResolveDiagnostic<Type>(
        Type diagnostic, string me, ConsoleColor? textColor = null)
        where Type : Diagnostic {
        var previous = Console.ForegroundColor;

        void ResetColor() {
            if (textColor != null)
                Console.ForegroundColor = textColor.Value;
            else
                Console.ResetColor();
        }

        var severity = diagnostic.info.severity;

        ResetColor();

        if (severity == DiagnosticType.Unknown) {
            // Ignore
        } else if (diagnostic.info.module != "AS" || (diagnostic is AraDiagnostic bd && bd.location == null)) {
            Console.Write($"{me}: ");

            if (severity == DiagnosticType.Warning) {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write("warning ");
            } else if (severity == DiagnosticType.Error) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("error ");
            } else if (severity == DiagnosticType.Fatal) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("fatal error ");
            }

            var errorCode = diagnostic.info.code.Value.ToString();
            errorCode = errorCode.PadLeft(4, '0');
            Console.Write($"{diagnostic.info.module}{errorCode}: ");

            ResetColor();
            Console.WriteLine(diagnostic.message);
        } else {
            PrettyPrintDiagnostic(diagnostic as AraDiagnostic, textColor);
        }

        Console.ForegroundColor = previous;

        return severity;
    }

    private static int ResolveDiagnostics<Type>(
        DiagnosticQueue<Type> diagnostics, string me, ConsoleColor? textColor = null)
        where Type : Diagnostic {
        if (diagnostics.count == 0)
            return SuccessExitCode;

        var worst = DiagnosticType.Unknown;
        var diagnostic = diagnostics.Pop();

        while (diagnostic != null) {
            var temp = ResolveDiagnostic(diagnostic, me, textColor);

            switch (temp) {
                case DiagnosticType.Warning:
                    if (worst == DiagnosticType.Unknown)
                        worst = temp;

                    break;
                case DiagnosticType.Error:
                    if (worst != DiagnosticType.Fatal)
                        worst = temp;

                    break;
                case DiagnosticType.Fatal:
                    worst = temp;
                    break;
            }

            diagnostic = diagnostics.Pop();
        }

        switch (worst) {
            case DiagnosticType.Error:
                return ErrorExitCode;
            case DiagnosticType.Fatal:
                return FatalExitCode;
            case DiagnosticType.Unknown:
            case DiagnosticType.Warning:
            default:
                return SuccessExitCode;
        }
    }

    private static int ResolveDiagnostics(Assembler assembler) {
        return ResolveDiagnostics(assembler, null);
    }

    private static int ResolveDiagnostics(
        Assembler assembler, string me = null, ConsoleColor textColor = ConsoleColor.White) {
        return ResolveDiagnostics(assembler.diagnostics, me ?? assembler.me, textColor);
    }

    private static void CleanOutputFiles(Assembler assembler) {
        if (assembler.state.finishStage == AssemblerStage.Linked) {
            if (File.Exists(assembler.state.outputFilename))
                File.Delete(assembler.state.outputFilename);

            return;
        }

        foreach (FileState file in assembler.state.tasks) {
            File.Delete(file.outputFilename);
        }
    }

    private static void ResolveOutputFiles(Assembler assembler) {
        // ProduceOutputFiles(assembler);
        CleanOutputFiles(assembler);
    }

    private static void ResolveAssemblerOutput(Assembler assembler) {
        if (assembler.state.finishStage == AssemblerStage.Linked) {
            if (assembler.state.linkOutputContent != null)
                File.WriteAllBytes(assembler.state.outputFilename, assembler.state.linkOutputContent.ToArray());

            return;
        }

        foreach (FileState file in assembler.state.tasks) {
            if (file.stage == assembler.state.finishStage)
                File.WriteAllBytes(file.outputFilename, file.fileContent.bytes.ToArray());
        }
    }

    private static void ReadInputFiles(Assembler assembler, out DiagnosticQueue<Diagnostic> diagnostics) {
        diagnostics = new DiagnosticQueue<Diagnostic>();

        for (int i=0; i<assembler.state.tasks.Length; i++) {
            ref var task = ref assembler.state.tasks[i];

            switch (task.stage) {
                case AssemblerStage.Assembled:
                    task.fileContent.bytes = File.ReadAllBytes(task.inputFilename).ToList();
                    break;
                case AssemblerStage.Linked:
                    diagnostics.Push(Ara.Diagnostics.Warning.IgnoringAssembledFile(task.inputFilename));
                    break;
                default:
                    break;
            }
        }
    }

    private static AssemblerState DecodeOptions(
        string[] args, out DiagnosticQueue<Diagnostic> diagnostics, out ShowDialogs dialogs) {
        var state = new AssemblerState();
        var tasks = new List<FileState>();
        var diagnosticsCL = new DiagnosticQueue<Diagnostic>();
        diagnostics = new DiagnosticQueue<Diagnostic>();

        var specifyStage = false;
        var specifyOut = false;

        var tempDialogs = new ShowDialogs();

        tempDialogs.help = false;
        tempDialogs.version = false;

        state.finishStage = AssemblerStage.Linked;
        state.outputFilename = "a.exe";

        void DecodeSimpleOption(string arg) {
            switch (arg) {
                case "-s":
                    specifyStage = true;
                    state.finishStage = AssemblerStage.Assembled;
                    break;
                case "-h":
                case "--help":
                    tempDialogs.help = true;
                    break;
                case "--version":
                    tempDialogs.version = true;
                    break;
                default:
                    diagnosticsCL.Push(Ara.Diagnostics.Error.UnrecognizedOption(arg));
                    break;
            }
        }

        for (int i=0; i<args.Length; i++) {
            var arg = args[i];

            if (!arg.StartsWith('-')) {
                diagnostics.Move(ResolveInputFileOrDir(arg, ref tasks));
                continue;
            }

            if (arg.StartsWith("-o")) {
                specifyOut = true;

                if (arg != "-o") {
                    state.outputFilename = arg.Substring(2);
                    continue;
                }

                if (i < args.Length - 1)
                    state.outputFilename = args[++i];
                else
                    diagnostics.Push(Ara.Diagnostics.Error.MissingFilenameO());
            } else {
                DecodeSimpleOption(arg);
            }
        }

        dialogs = tempDialogs;
        diagnostics.Move(diagnosticsCL);

        if (dialogs.help || dialogs.version)
            return state;

        state.tasks = tasks.ToArray();

        if (specifyOut && specifyStage && state.tasks.Length > 1)
            diagnostics.Push(Ara.Diagnostics.Error.CannotSpecifyWithMultipleFiles());

        state.outputFilename = state.outputFilename.Trim();

        return state;
    }

    private static DiagnosticQueue<Diagnostic> ResolveInputFileOrDir(string name, ref List<FileState> tasks) {
        var filenames = new List<string>();
        var diagnostics = new DiagnosticQueue<Diagnostic>();

        if (Directory.Exists(name)) {
            filenames.AddRange(Directory.GetFiles(name));
        } else if (File.Exists(name)) {
            filenames.Add(name);
        } else {
            diagnostics.Push(Ara.Diagnostics.Error.NoSuchFileOrDirectory(name));
            return diagnostics;
        }

        foreach (string filename in filenames) {
            var task = new FileState();
            task.inputFilename = filename;

            var parts = task.inputFilename.Split('.');
            var type = parts[parts.Length - 1];

            switch (type) {
                case "s":
                    task.stage = AssemblerStage.Raw;
                    break;
                case "o":
                case "obj":
                    task.stage = AssemblerStage.Assembled;
                    break;
                default:
                    diagnostics.Push(Ara.Diagnostics.Warning.IgnoringUnknownFileType(task.inputFilename));
                    continue;
            }

            tasks.Add(task);
        }

        return diagnostics;
    }
}
