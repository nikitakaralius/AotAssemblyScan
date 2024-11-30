namespace AotAssemblyScan.Tests;

public class AssemblyScanGeneratorTests
{
    // requires attribute

    // requires partial modifier

    // when only assembly scan attribute specified should return all assembly types

    // generates code for methods with similar name

    // does not generate code for method with parameters

    // generates code only for IReadOnlyList (handle IEnumerable, ICollection, IList, etc. in the future)

    // includes the type itself in IsAssignableTo

    // finds inheritors in IsAssignableTo

    // combines IsAssignableTo using &&

    // finds types with HasAttribute

    // combines HasAttribute using &&

    // includes only abstract classes when using IsAbstract

    // filters abstract classes when using IsAbstract(false)

    // includes only interfaces when using IsInterface

    // filters interfaces when using IsInterface(false)
}
