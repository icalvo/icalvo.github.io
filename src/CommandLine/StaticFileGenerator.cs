using SWGen;

namespace CommandLine;

public class StaticFileGenerator : Generator
{
    public override IEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, ISwgLogger logger, CancellationToken ct)
    {
        yield return new(inputFile, () => Task.FromResult((Stream)File.OpenRead((projectRoot / inputFile).Normalized())));
    }

    public override bool SkipWriteIfFileExists() => true;
}