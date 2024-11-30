namespace AotAssemblyScan.Sample;

public static partial class AssemblyExtensions
{
    [AssemblyScan]
    [IsInterface(false), IsAbstract(false)]
    [HasAttribute<MarkerAttribute>, HasAttribute<Marker2Attribute>]
    [Implements<IMarkerInterface>, Implements<IMarkerInterface2>]
    public static partial IReadOnlyList<Type> GetMarkedTypes();
}
