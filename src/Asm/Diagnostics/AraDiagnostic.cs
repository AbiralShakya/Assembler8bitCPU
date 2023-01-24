using Asm.CodeAnalysis.Text;
using Diagnostics;

namespace Asm.Diagnostics;

/// <summary>
/// Ara specific <see cref="Diagnostic" />.
/// </summary>
public sealed class AraDiagnostic : Diagnostic {
    /// <summary>
    /// Creates a <see cref="AraDiagnostic" />.
    /// </summary>
    /// <param name="info">Severity and code of <see cref="AraDiagnostic" />.</param>
    /// <param name="location">Location of the <see cref="AraDiagnostic" />.</param>
    /// <param name="message">Message/info on the <see cref="AraDiagnostic" />.</param>
    /// <param name="suggestion">A possible solution to the problem.</param>
    public AraDiagnostic(DiagnosticInfo info, TextLocation location, string message, string suggestion)
        : base (info, message, suggestion) {
        this.location = location;
    }

    /// <summary>
    /// Creates a <see cref="AraDiagnostic" /> without a suggestion.
    /// </summary>
    /// <param name="info">Severity and code of <see cref="AraDiagnostic" />.</param>
    /// <param name="location">Location of the <see cref="AraDiagnostic" />.</param>
    /// <param name="message">Message/info on the <see cref="AraDiagnostic" />.</param>
    public AraDiagnostic(DiagnosticInfo info, TextLocation location, string message)
        : this(info, location, message, null) { }

    /// <summary>
    /// Creates a <see cref="AraDiagnostic" /> using a severity instead of <see cref="DiagnosticInfo" />,
    /// no suggestion.
    /// </summary>
    /// <param name="type">Severity of <see cref="AraDiagnostic" />.</param>
    /// <param name="location">Location of the <see cref="AraDiagnostic" />.</param>
    /// <param name="message">Message/info on the <see cref="AraDiagnostic" />.</param>
    public AraDiagnostic(DiagnosticType type, TextLocation location, string message)
        : this(new DiagnosticInfo(type), location, message, null) { }

    /// <summary>
    /// Creates a <see cref="AraDiagnostic" /> using a severity instead of <see cref="DiagnosticInfo" />,
    /// no suggestion, and  no location.
    /// </summary>
    /// <param name="type">Severity of <see cref="AraDiagnostic" />.</param>
    /// <param name="message">Message/info on the <see cref="AraDiagnostic" />.</param>
    public AraDiagnostic(DiagnosticType type, string message)
        : this(new DiagnosticInfo(type), null, message, null) { }

    /// <summary>
    /// Creates a <see cref="AraDiagnostic" /> without a location or suggestion.
    /// </summary>
    /// <param name="info">Severity and code of <see cref="AraDiagnostic" />.</param>
    /// <param name="message">Message/info on the <see cref="AraDiagnostic" />.</param>
    public AraDiagnostic(DiagnosticInfo info, string message)
        : this(info, null, message, null) { }

    /// <summary>
    /// Creates a <see cref="AraDiagnostic" /> from an existing <see cref="AraDiagnostic" /> (copies).
    /// </summary>
    /// <param name="diagnostic"><see cref="AraDiagnostic" /> to copy (soft copy).</param>
    public AraDiagnostic(Diagnostic diagnostic)
        : this(diagnostic.info, null, diagnostic.message, diagnostic.suggestion) { }

    /// <summary>
    /// Where the <see cref="AraDiagnostic" /> is in the source code (what code produced the
    /// <see cref="AraDiagnostic" />).
    /// </summary>
    public TextLocation location { get; }
}
