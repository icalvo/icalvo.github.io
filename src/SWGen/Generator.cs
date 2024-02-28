namespace SWGen;

public abstract class Generator
{
    public abstract IAsyncEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, CancellationToken ct);

    public virtual bool SkipWriteIfFileExists() => false;
}