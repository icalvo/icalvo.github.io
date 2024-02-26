using System.Runtime.CompilerServices;
using SWGen;

namespace CommandLine;

public class StaticFileGenerator : Generator
{
    public override async IAsyncEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx page, [EnumeratorCancellation] CancellationToken ct)
    {
        yield return new(page, await File.ReadAllBytesAsync((projectRoot / page).Normalized(), ct));
    }
}