using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.TypeFilters;

public sealed class IsInterfaceTypeFilter : ITypeFilter
{
    private const string AttributeTypeName = "IsInterfaceAttribute";
    private bool? _isInterface;

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

        _isInterface = isAbstract;

        return true;
    }

    public bool Matches(INamedTypeSymbol type)
    {
        return _isInterface switch
        {
            null => true,
            true => type.TypeKind == TypeKind.Interface,
            false => type.TypeKind != TypeKind.Interface
        };
    }
}
