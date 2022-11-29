using System.Collections.Generic;
using System.Linq;
using Diagnostics;

namespace Asm.Diagnostics;

/// <summary>
/// Diagnostic queue for AraDiagnostics.
/// </summary>
public sealed class AraDiagnosticQueue : DiagnosticQueue<AraDiagnostic> {
    /// <summary>
    /// Creates a queue with no diagnostics.
    /// </summary>
    public AraDiagnosticQueue() : base() { }

    /// <summary>
    /// Creates a queue with diagnostics (ordered from oldest -> newest).
    /// </summary>
    /// <param name="diagnostics">Diagnostics to copy into queue initially</param>
    public AraDiagnosticQueue(IEnumerable<AraDiagnostic> diagnostics) : base(diagnostics) { }

    /// <summary>
    /// Sorts, removes duplicates, and modifies diagnostics.
    /// </summary>
    /// <param name="diagnostics">Queue to copy then clean, does not modify queue</param>
    /// <returns>New cleaned queue</returns>
    public static AraDiagnosticQueue CleanDiagnostics(AraDiagnosticQueue diagnostics) {
        var cleanedDiagnostics = new AraDiagnosticQueue();
        var specialDiagnostics = new AraDiagnosticQueue();

        var diagnosticList = diagnostics.AsList<AraDiagnostic>();

        for (int i=0; i<diagnosticList.Count; i++) {
            var diagnostic = diagnosticList[i];

            if (diagnostic.location == null) {
                specialDiagnostics.Push(diagnostic);
                diagnosticList.RemoveAt(i--);
            }
        }

        foreach (var diagnostic in diagnosticList.OrderBy(diag => diag.location.fileName)
                .ThenBy(diag => diag.location.span.start)
                .ThenBy(diag => diag.location.span.length)) {
            cleanedDiagnostics.Push(diagnostic);
        }

        cleanedDiagnostics.Move(specialDiagnostics);
        return cleanedDiagnostics;
    }

    /// <summary>
    /// Copies queue without a specific severity of diagnostic.
    /// </summary>
    /// <param name="type">Severity to not copy (see DiagnosticType)</param>
    /// <returns>New, unlinked queue</returns>
    public new AraDiagnosticQueue FilterOut(DiagnosticType type) {
        return new AraDiagnosticQueue(AsList().Where(d => d.info.severity != type));
    }
}

/// <summary>
/// Ara specific diagnostic.
/// </summary>
public sealed class AraDiagnostic : Diagnostic {
    /// <summary>
    /// Creates a diagnostic.
    /// </summary>
    /// <param name="info">Severity and code of diagnostic</param>
    /// <param name="location">Location of the diagnostic</param>
    /// <param name="message">Message/info on the diagnostic</param>
    /// <param name="suggestion">A possible solution to the problem</param>
    public AraDiagnostic(DiagnosticInfo info, TextLocation location, string message, string suggestion)
        : base (info, message, suggestion) {
        this.location = location;
    }

    /// <summary>
    /// Creates a diagnostic without a suggestion.
    /// </summary>
    /// <param name="info">Severity and code of diagnostic</param>
    /// <param name="location">Location of the diagnostic</param>
    /// <param name="message">Message/info on the diagnostic</param>
    public AraDiagnostic(DiagnosticInfo info, TextLocation location, string message)
        : this(info, location, message, null) { }

    /// <summary>
    /// Creates a diagnostic using a severity instead of DiagnosticInfo, no suggestion.
    /// </summary>
    /// <param name="type">Severity of diagnostic</param>
    /// <param name="location">Location of the diagnostic</param>
    /// <param name="message">Message/info on the diagnostic</param>
    public AraDiagnostic(DiagnosticType type, TextLocation location, string message)
        : this(new DiagnosticInfo(type), location, message, null) { }

    /// <summary>
    /// Creates a diagnostic using a severity instead of DiagnosticInfo, no suggestion, and  no location.
    /// </summary>
    /// <param name="type">Severity of diagnostic</param>
    /// <param name="message">Message/info on the diagnostic</param>
    public AraDiagnostic(DiagnosticType type, string message)
        : this(new DiagnosticInfo(type), null, message, null) { }

    /// <summary>
    /// Creates a diagnostic without a location or suggestion.
    /// </summary>
    /// <param name="info">Severity and code of diagnostic</param>
    /// <param name="message">Message/info on the diagnostic</param>
    public AraDiagnostic(DiagnosticInfo info, string message)
        : this(info, null, message, null) { }

    /// <summary>
    /// Creates a diagnostic from an existing diagnostic (copies).
    /// </summary>
    /// <param name="diagnostic">Diagnostic to copy (soft copy)</param>
    public AraDiagnostic(Diagnostic diagnostic)
        : this(diagnostic.info, null, diagnostic.message, diagnostic.suggestion) { }

    /// <summary>
    /// Where the diagnostic is in the source code (what code produced the diagnostic).
    /// </summary>
    public TextLocation location { get; }
}
