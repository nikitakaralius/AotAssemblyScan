using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.TypeCriteria;

public sealed class IsAssignableToTypeCriteria : ITypeCriteria
{
    private const string AttributeTypeName = "IsAssignableToAttribute";

    private readonly List<INamedTypeSymbol> _assignableToTypes = [];

    public bool TryRegisterFor(AttributeData attribute)
    {
        var attributeClass = attribute.AttributeClass;

        if (attributeClass is null)
            return false;

        if (attributeClass.Name != AttributeTypeName)
            return false;

        if (!attributeClass.IsGenericType)
            return false;

        _assignableToTypes.Add((INamedTypeSymbol) attributeClass.TypeArguments[0]);

        return true;
    }

    public bool Satisfies(INamedTypeSymbol type)
    {
        var typesToCheck = type.AllInterfaces.Add(type);

        if (type.BaseType is not null)
            typesToCheck = typesToCheck.Add(type.BaseType);

        return _assignableToTypes
           .All(typeFromAttribute => typesToCheck
               .Any(baseType => SymbolEqualityComparer.Default.Equals(typeFromAttribute, baseType)));
    }
}
