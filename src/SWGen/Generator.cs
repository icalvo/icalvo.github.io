namespace SWGen;

public abstract class Generator
{
    public abstract IEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, CancellationToken ct);

    public virtual bool SkipWriteIfFileExists() => false;
}