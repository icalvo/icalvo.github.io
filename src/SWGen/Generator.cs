namespace SWGen;

public abstract class Generator
{
    public abstract IEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, ISwgLogger logger, CancellationToken ct);

    public virtual bool SkipWriteIfFileExists() => false;
}