#pragma warning disable CS0414 // Field is assigned but its value is never used

using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.TypeFilters;

public sealed class IsAbstractTypeFilter : ITypeFilter
{
    private const string AttributeTypeName = "IsAbstract";
    private bool? _isAbstract = null;

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
