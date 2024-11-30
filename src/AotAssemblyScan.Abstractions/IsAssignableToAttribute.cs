namespace AotAssemblyScan;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class IsAssignableToAttribute<TInterface> : Attribute;
