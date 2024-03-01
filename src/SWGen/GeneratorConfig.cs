namespace SWGen;

public record GeneratorConfig(Generator Generator, GeneratorTrigger Trigger)
{
    public bool MatchesFile(AbsolutePathEx projectRoot, RelativePathEx page) =>
        Trigger switch
        {
            GeneratorTrigger.Once => false,
            GeneratorTrigger.OnFilePredicate t => t.Predicate(projectRoot, page),
            _ => throw new ArgumentOutOfRangeException()
        };
}