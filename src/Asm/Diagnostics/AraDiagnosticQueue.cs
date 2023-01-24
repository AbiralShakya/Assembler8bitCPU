using Diagnostics;

namespace Asm.Diagnostics;

/// <summary>
/// A <see cref="DiagnosticQueue" /> containing <see cref="AraDiagnostic" />s.
/// </summary>
public sealed class AraDiagnosticQueue : DiagnosticQueue<AraDiagnostic> {
    /// <summary>
    /// Creates a <see cref="AraDiagnosticQueue" /> with no Diagnostics.
    /// </summary>
    public AraDiagnosticQueue() : base() { }

    /// <summary>
    /// Creates a <see cref="AraDiagnosticQueue" /> with Diagnostics (ordered from oldest -> newest).
    /// </summary>
    /// <param name="diagnostics">Diagnostics to copy into <see cref="AraDiagnosticQueue" /> initially.</param>
    public AraDiagnosticQueue(IEnumerable<AraDiagnostic> diagnostics) : base(diagnostics) { }

    /// <summary>
    /// Sorts, removes duplicates, and modifies Diagnostics.
    /// </summary>
    /// <param name="diagnostics"><see cref="AraDiagnosticQueue" /> to copy then clean, does not modify
    /// <see cref="AraDiagnosticQueue" />.</param>
    /// <returns>New cleaned <see cref="AraDiagnosticQueue" />.</returns>
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
    /// Copies <see cref="AraDiagnosticQueue" /> without a specific severity of <see cref="AraDiagnostic" />.
    /// </summary>
    /// <param name="type">Severity to not copy (see <see cref="DiagnosticType" />).</param>
    /// <returns>New, unlinked <see cref="AraDiagnosticQueue" />.</returns>
    public new AraDiagnosticQueue FilterOut(DiagnosticType type) {
        return new AraDiagnosticQueue(AsList().Where(d => d.info.severity != type));
    }
}
