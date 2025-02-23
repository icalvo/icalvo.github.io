using SWGen.FileSystems;

namespace SWGen.Generators;

public abstract class Generator
{
    public abstract IEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, ISwgLogger logger, CancellationToken ct);
}