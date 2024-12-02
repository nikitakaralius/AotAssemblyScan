using AotAssemblyScan.Sample.Options.Data.Abstractions;

namespace AotAssemblyScan.Sample.Options.Data.Options;

public sealed record AuthOptions : IApplicationOptions
{
    public static string Section { get; } = "Auth";

    public required Uri AuthorityUrl { get; init; } = null!;

    public required IReadOnlyList<string> Audiences { get; init; } = [];
}
