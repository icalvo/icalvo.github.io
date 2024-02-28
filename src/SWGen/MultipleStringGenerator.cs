using System.Text;

namespace SWGen;

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

    protected abstract IEnumerable<(string File, Func<Task<string>> Content)> GenerateString(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx page, CancellationToken _);
    public override IEnumerable<GeneratorItem> Generate(
        SiteContents ctx,
        AbsolutePathEx projectRoot,
        RelativePathEx inputFile, CancellationToken ct) =>
        GenerateString(ctx, projectRoot, inputFile, ct)
            .Select(x => new GeneratorItem(x.File, async () => new MemoryStream(_encoding.GetBytes(await x.Content()))));
}