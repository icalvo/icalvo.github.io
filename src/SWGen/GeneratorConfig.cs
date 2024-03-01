namespace SWGen;

public record GeneratorConfig(Generator Generator, Func<AbsolutePathEx, RelativePathEx, bool> MatchesFile)
{
}