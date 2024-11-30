namespace AotAssemblyScan.Sample;

public static partial class AssemblyExtensions
{
    [AssemblyScan]
    // [IsInterface(false), IsAbstract(false)]
    // [HasAttribute<MarkerAttribute>, HasAttribute<Marker2Attribute>]
    [IsAssignableTo<MarkedClass>]
    // [IsAssignableTo<IMarkerInterface2>]
    public static partial IReadOnlyList<Type> GetMarkedTypes();
}
