namespace AotAssemblyScan.Sample;

[Marker, Marker2]
public class MarkedClass : IMarkerInterface, IMarkerInterface2;

[Marker, Marker2]
public abstract class AbstractMarkedClass : IMarkerInterface, IMarkerInterface2;
