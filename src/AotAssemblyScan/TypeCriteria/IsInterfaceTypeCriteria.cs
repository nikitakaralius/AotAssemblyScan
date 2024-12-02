using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.TypeCriteria;

public sealed class IsInterfaceTypeCriteria : ITypeCriteria
{
    private const string AttributeTypeName = "IsInterfaceAttribute";
    private bool? _isInterface;

    public bool TryRegisterFor(AttributeData attribute)
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

    public bool Satisfies(INamedTypeSymbol type)
    {
        return _isInterface switch
        {
            null => true,
            true => type.TypeKind == TypeKind.Interface,
            false => type.TypeKind != TypeKind.Interface
        };
    }
}
