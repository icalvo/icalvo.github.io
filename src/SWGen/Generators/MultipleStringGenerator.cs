using System.IO;
using System.Text;
using SWGen.FileSystems;

namespace SWGen.Generators;

public abstract class MultipleStringGenerator : Generator
{
    private readonly Encoding _encoding;

    protected MultipleStringGenerator()
    {
        _encoding = Encoding.UTF8;
    }

    protected MultipleStringGenerator(Encoding encoding)
    {
        _encoding = encoding;
    }

    protected abstract IEnumerable<(RelativePathEx File, Func<Task<string>> Content)> GenerateString(
        SiteContents ctx,
        AbsolutePathEx projectRoot,
        RelativePathEx inputFile,
        ISwgLogger logger,
        CancellationToken _);
    public override IEnumerable<GeneratorItem> Generate(
        SiteContents ctx,
        AbsolutePathEx projectRoot,
        RelativePathEx inputFile,
        ISwgLogger logger,
        CancellationToken ct) =>
        GenerateString(ctx, projectRoot, inputFile, logger, ct)
            .Select(x => new GeneratorItem(x.File, async () => new MemoryStream(_encoding.GetBytes(await x.Content()))));
}