namespace AotAssemblyScan.Abstractions;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IsInterfaceAttribute(bool isInterface = true) : Attribute;
