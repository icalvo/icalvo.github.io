using System.Runtime.CompilerServices;
using System.Text;

namespace SWGen;

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

    protected abstract Task<(RelativePathEx Link, string Content)> GenerateString(SiteContents ctx,
        AbsolutePathEx projectRoot, RelativePathEx page, CancellationToken ct);

    public override async IAsyncEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, [EnumeratorCancellation] CancellationToken ct)
    {
        var (link, content) = await GenerateString(ctx, projectRoot, inputFile, ct);
        yield return new(link, new MemoryStream(_encoding.GetBytes(content)));
    }
}