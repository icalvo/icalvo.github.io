﻿using System.Runtime.CompilerServices;
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

    protected abstract (RelativePathEx Link, Func<Task<string>> Content) GenerateString(SiteContents ctx,
        AbsolutePathEx projectRoot, RelativePathEx page, CancellationToken ct);

    public override IEnumerable<GeneratorItem> Generate(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, CancellationToken ct)
    {
        var (link, content) = GenerateString(ctx, projectRoot, inputFile, ct);
        yield return new(link, async () => new MemoryStream(_encoding.GetBytes(await content())));
    }
}