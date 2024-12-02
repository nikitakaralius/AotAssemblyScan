using System.Reflection;
using AotAssemblyScan.Sample.Options.Data.Abstractions;

namespace AotAssemblyScan.Sample.Options.Extensions;

public static partial class ConfigurationExtensions
{
    private delegate IServiceCollection Configure(IServiceCollection services, IConfiguration configuration);

    private static MethodInfo ConfigureMethodInfo { get; }

    static ConfigurationExtensions()
    {
        Type[] parameterTypes = [typeof(IServiceCollection), typeof(IConfiguration)];
        const string methodName = nameof(OptionsConfigurationServiceCollectionExtensions.Configure);

        ConfigureMethodInfo = typeof(OptionsConfigurationServiceCollectionExtensions)
           .GetMethod(methodName, parameterTypes)!;
    }

    public static IServiceCollection AddApplicationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        foreach (var type in GetConfigurationTypesUsingAotAssemblyScan())
        {
            var property = type.GetProperty(
                nameof(IApplicationOptions.Section),
                BindingFlags.Public | BindingFlags.Static);

            string value = (string) property!.GetValue(null)!;
            var section = configuration.GetRequiredSection(value);

            var configure = BuildConfigureMethod(type);

            configure(services, section);
        }

        return services;
    }

    public static Type[] GetConfigurationTypesUsingReflection() =>
        Assembly
           .GetExecutingAssembly()
           .GetTypes()
           .Where(t => t.IsAssignableTo(typeof(IApplicationOptions)))
           .Where(t => t is {IsInterface: false, IsAbstract: false})
           .ToArray();

    [AssemblyScan]
    [IsAssignableTo<IApplicationOptions>]
    [IsInterface(false), IsAbstract(false)]
    public static partial Type[] GetConfigurationTypesUsingAotAssemblyScan();

    private static Configure BuildConfigureMethod(Type type)
    {
        const object? staticTarget = null;

        return ConfigureMethodInfo
           .MakeGenericMethod(type)
           .CreateDelegate<Configure>(staticTarget);
    }
}
