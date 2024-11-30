using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.Extensions;

public static class SymbolExtensions
{
    // TODO: check for namespace as well
    public static bool HasAttribute(this ISymbol symbol, string attributeName)
    {
        return symbol
           .GetAttributes()
           .Any(attr => attr.AttributeClass?.Name == attributeName);
    }
}
