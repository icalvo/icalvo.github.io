using System.Runtime.CompilerServices;
using SWGen;

namespace CommandLine;

public class StaticFileGenerator : Generator
{
    public override async IAsyncEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, [EnumeratorCancellation] CancellationToken ct)
    {
        yield return new(inputFile, File.OpenRead((projectRoot / inputFile).Normalized()));
    }

    public override bool SkipWriteIfFileExists() => true;
}