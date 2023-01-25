
namespace Asm.CodeAnalysis.Symbols;

/// <summary>
/// A type symbol.
/// </summary>
internal class TypeSymbol : Symbol {
    /// <summary>
    /// Error type (meaning something went wrong, not an actual type).
    /// </summary>
    internal static readonly TypeSymbol Error = new TypeSymbol("?");

    /// <summary>
    /// Integer type (any whole number, signed).
    /// </summary>
    internal static readonly TypeSymbol Byte = new TypeSymbol("byte");

    /// <summary>
    /// Creates a new <see cref="TypeSymbol" />.
    /// Use predefined type symbols if possible.
    /// </summary>
    /// <param name="name">Name of type.</param>
    internal TypeSymbol(string name) : base(name) { }

    internal override SymbolKind kind => SymbolKind.Type;
}
