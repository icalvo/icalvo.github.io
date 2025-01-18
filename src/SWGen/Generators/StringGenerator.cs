using System.IO;
using System.Text;
using SWGen.FileSystems;

namespace SWGen.Generators;

public abstract class StringGenerator : Generator
{
    private readonly Encoding _encoding;

    protected StringGenerator()
    {
        _encoding = Encoding.UTF8;
    }

    protected StringGenerator(Encoding encoding)
    {
        _encoding = encoding;
    }

    protected abstract (RelativePathEx Link, Func<Task<string>> Content) GenerateString(SiteContents ctx,
        AbsolutePathEx projectRoot, RelativePathEx page, ISwgLogger logger, CancellationToken ct);

    public override IEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, ISwgLogger logger, CancellationToken ct)
    {
        var (link, content) = GenerateString(ctx, projectRoot, inputFile, logger, ct);
        yield return new(link, async () => new MemoryStream(_encoding.GetBytes(await content())));
    }
}