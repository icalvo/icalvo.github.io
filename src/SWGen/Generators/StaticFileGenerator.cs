using System.IO;
using SWGen.FileSystems;

namespace SWGen.Generators;

public class StaticFileGenerator : Generator
{
    private readonly IFileSystem _fs;

    public StaticFileGenerator(IFileSystem fs)
    {
        _fs = fs;
    }

    public override IEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, ISwgLogger logger, CancellationToken ct)
    {
        yield return new(inputFile, () => Task.FromResult((Stream)File.OpenRead((projectRoot / inputFile).Normalized(_fs))));
    }

    public override bool SkipWriteIfFileExists() => true;
}