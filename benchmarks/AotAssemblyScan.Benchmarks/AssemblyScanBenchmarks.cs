using AotAssemblyScan.Sample.Options.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace AotAssemblyScan.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class AssemblyScanBenchmarks
{
    [Benchmark]
    public Type[] Reflection()
    {
        var types = ConfigurationExtensions.GetConfigurationTypesUsingReflection();
        return types;
    }

    [Benchmark]
    public Type[] AotAssemblyScan()
    {
        var types = ConfigurationExtensions.GetConfigurationTypesUsingAotAssemblyScan();
        return types;
    }
}
