namespace AotAssemblyScan.Abstractions;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ImplementsAttribute<TInterface> : Attribute;
