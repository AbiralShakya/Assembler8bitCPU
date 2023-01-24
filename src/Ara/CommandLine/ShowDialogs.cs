
namespace Ara.CommandLine;

/// <summary>
/// Flags that tell the <see cref="AraCommandLine" /> what dialogs to display.
/// </summary>
public struct ShowDialogs {
    /// <summary>
    /// Display help dialog.
    /// </summary>
    public bool help;

    /// <summary>
    /// Display compiler version information dialog.
    /// </summary>
    public bool version;
}
