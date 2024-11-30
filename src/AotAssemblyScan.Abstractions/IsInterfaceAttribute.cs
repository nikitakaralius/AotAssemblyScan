namespace AotAssemblyScan;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IsInterfaceAttribute(bool isInterface = true) : Attribute;
