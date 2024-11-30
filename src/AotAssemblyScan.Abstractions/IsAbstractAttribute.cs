namespace AotAssemblyScan;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IsAbstractAttribute(bool isAbstract = true) : Attribute;
