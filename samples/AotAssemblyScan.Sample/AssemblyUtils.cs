namespace AotAssemblyScan.Sample;

public static partial class AssemblyUtils
{
    [AssemblyScan]
    [IsInterface(false), IsAbstract(false)]
    [HasAttribute<MarkerAttribute>, HasAttribute<Marker2Attribute>]
    [IsAssignableTo<IMarkerInterface>, IsAssignableTo<IMarkerInterface2>]
    public static partial IReadOnlyList<Type> GetMarkedTypes();
}
