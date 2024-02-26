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

    protected abstract (RelativePathEx Link, string Content) GenerateString(SiteContents ctx, AbsolutePathEx projectRoot, RelativePathEx page);

    public override GeneratorItem[] Generate(SiteContents ctx, AbsolutePathEx projectRoot, RelativePathEx page)
    {
        var (link, content) = GenerateString(ctx, projectRoot, page);
        return [new(link, _encoding.GetBytes(content))];
    }
}