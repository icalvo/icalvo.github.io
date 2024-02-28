namespace SWGen;

public record GeneratorConfig(Generator Generator, GeneratorTrigger Trigger)
{
    public bool MatchesFile(AbsolutePathEx projectRoot, RelativePathEx page) =>
        Trigger switch
        {
            GeneratorTrigger.Once => false,
            GeneratorTrigger.OnFile t => t.FileName.Equals(page),
            GeneratorTrigger.OnFileExt t => t.Extension == page.Extension,
            GeneratorTrigger.OnFilePredicate t => t.Predicate(projectRoot, page),
            _ => throw new ArgumentOutOfRangeException()
        };
}