using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.TypeFilters;

public sealed class HasAttributeTypeFilter : ITypeFilter
{
    private const string AttributeTypeName = "HasAttributeAttribute";

    private readonly List<INamedTypeSymbol> _attributesToHave = [];

    public bool TryAdd(AttributeData attribute)
    {
        var attributeClass = attribute.AttributeClass;

        if (attributeClass is null)
            return false;

        if (attributeClass.Name != AttributeTypeName)
            return false;

        if (!attributeClass.IsGenericType)
            return false;

        _attributesToHave.Add((INamedTypeSymbol) attributeClass.TypeArguments[0]);

        return true;
    }

    public bool Matches(INamedTypeSymbol type)
    {
        return _attributesToHave
           .All(attributeToHave => type
               .GetAttributes()
               .Any(typeAttribute => SymbolEqualityComparer
                   .Default
                   .Equals(typeAttribute.AttributeClass, attributeToHave)));
    }
}
