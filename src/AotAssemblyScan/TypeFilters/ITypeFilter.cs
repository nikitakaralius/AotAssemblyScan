using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.TypeFilters;

public interface ITypeFilter
{
    bool TryAdd(AttributeData attribute);

    bool Matches(INamedTypeSymbol type);
}
