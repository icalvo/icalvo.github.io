using System.Runtime.CompilerServices;

namespace SWGen;

public record GeneratorConfig
{
    private readonly string? _matcherName;

    public GeneratorConfig(Generator generator, Func<AbsolutePathEx, RelativePathEx, bool> matchesFile, [CallerArgumentExpression(nameof(matchesFile))] string? argumentName = null)
    {
        _matcherName = argumentName ?? "unknown";
        Generator = generator;
        MatchesFile = matchesFile;
    }

    public Generator Generator { get; init; }
    public Func<AbsolutePathEx, RelativePathEx, bool> MatchesFile { get; init; }

    public override string ToString()
    {
        return $"GeneratorConfig {{ Generator = {Generator.GetType().Name}, Matcher = {_matcherName} }}";
    }

    public void Deconstruct(out Generator generator, out Func<AbsolutePathEx, RelativePathEx, bool> matchesFile)
    {
        generator = Generator;
        matchesFile = MatchesFile;
    }
}