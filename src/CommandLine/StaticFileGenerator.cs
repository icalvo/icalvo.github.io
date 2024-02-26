using SWGen;

namespace CommandLine;

public class StaticFileGenerator : Generator
{
    public override GeneratorItem[] Generate(SiteContents ctx, AbsolutePathEx projectRoot, RelativePathEx page) =>
        [new(page, File.ReadAllBytes((projectRoot / page).Normalized()))];
}