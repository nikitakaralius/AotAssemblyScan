using Microsoft.CodeAnalysis;

namespace AotAssemblyScan.TypeCriteria;

public interface ITypeCriteria
{
    bool TryRegisterFor(AttributeData attribute);

    bool Satisfies(INamedTypeSymbol type);
}
