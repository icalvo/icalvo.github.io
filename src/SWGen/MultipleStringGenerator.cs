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

    protected abstract IEnumerable<(string, string)> GenerateString(SiteContents ctx, AbsolutePathEx projectRoot, RelativePathEx page);
    public override GeneratorItem[] Generate(SiteContents ctx, AbsolutePathEx projectRoot, RelativePathEx page) =>
        GenerateString(ctx, projectRoot, page)
            .Select(x => new GeneratorItem(x.Item1, _encoding.GetBytes(x.Item2)))
            .ToArray();
}