namespace AotAssemblyScan.Sample.Options.Data.Options;

public sealed record ServiceOptions
{
    public static string Section { get; } = "Service";

    public required string Url { get; init; }

    public required Guid Key { get; init; }

    public required int RequestsPerHour { get; init; }
}
