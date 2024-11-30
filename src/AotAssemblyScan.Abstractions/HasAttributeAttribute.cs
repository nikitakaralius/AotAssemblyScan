namespace AotAssemblyScan;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasAttributeAttribute<TInterface> : Attribute;