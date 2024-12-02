using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.TypeCriteria;

public sealed class HasAttributeTypeCriteria : ITypeCriteria
{
    private const string AttributeTypeName = "HasAttributeAttribute";

    private readonly List<INamedTypeSymbol> _attributesToHave = [];

    public bool TryRegisterFor(AttributeData attribute)
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

    public bool Satisfies(INamedTypeSymbol type)
    {
        return _attributesToHave
           .All(attributeToHave => type
               .GetAttributes()
               .Any(typeAttribute => SymbolEqualityComparer
                   .Default
                   .Equals(typeAttribute.AttributeClass, attributeToHave)));
    }
}
