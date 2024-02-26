namespace SWGen;

public abstract class Generator
{
    public abstract GeneratorItem[] Generate(SiteContents ctx, AbsolutePathEx projectRoot, RelativePathEx page);
}