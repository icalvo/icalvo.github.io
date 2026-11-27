using System.IO;
using SWGen.FileSystems;

namespace SWGen.Generators;

public class StaticFileGenerator(IFileSystem fs) : Generator
{
    public override IEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, ISwgLogger logger, CancellationToken ct)
    {
        yield return new(
            inputFile,
            () => Task.FromResult((Stream)File.OpenRead((projectRoot / inputFile).Normalized(fs))),
            SkipIfExists: inputFile.Extension != ".css");
    }
}
