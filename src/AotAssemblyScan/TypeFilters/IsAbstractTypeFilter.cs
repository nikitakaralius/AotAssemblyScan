using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.TypeFilters;

public sealed class IsAbstractTypeFilter : ITypeFilter
{
    private const string AttributeTypeName = "IsAbstractAttribute";
    private bool? _isAbstract;

    public bool TryAdd(AttributeData attribute)
    {
        var attributeClass = attribute.AttributeClass;

        if (attributeClass is null)
            return false;

        if (attributeClass.Name != AttributeTypeName)
            return false;

        if (attribute.ConstructorArguments.Length == 0)
            return false;

        if (attribute.ConstructorArguments[0].Value is not bool isAbstract)
            return false;

        _isAbstract = isAbstract;

        return true;
    }

    public bool Matches(INamedTypeSymbol type)
    {
        if (_isAbstract is null)
            return true;

        return type.IsAbstract == _isAbstract;
    }
}
