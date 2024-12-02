using AotAssemblyScan.Sample.Options.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace AotAssemblyScan.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[BaselineColumn]
public class AssemblyScanBenchmarks
{
    [Benchmark(Baseline = true)]
    public Type[] AotAssemblyScan()
    {
        var types = ConfigurationExtensions.GetConfigurationTypesUsingAotAssemblyScan();
        return types;
    }

    [Benchmark]
    public Type[] Reflection()
    {
        var types = ConfigurationExtensions.GetConfigurationTypesUsingReflection();
        return types;
    }
}
