﻿using SWGen;
using SWGen.FileSystems;

namespace CommandLine;

public class SiteMapGenerator : StringGenerator
{
    private readonly IRazorEngineFactory _razorEngineFactory;
    private readonly FileSystem _fs;

    public SiteMapGenerator(IRazorEngineFactory razorEngineFactory, FileSystem fs)
    {
        _razorEngineFactory = razorEngineFactory;
        _fs = fs;
    }

    protected override (RelativePathEx, Func<Task<string>>) GenerateString(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx page, ISwgLogger logger, CancellationToken ct)
    {
        return ("sitemap.xml", Func);

        async Task<string> Func()
        {
            var engine = _razorEngineFactory.Create(projectRoot.Normalized(_fs));
            var posts = ctx.TryGetValues<Document<Post>>().OrderByDescending(p => p.Metadata.Published);
        
            var doc = new Document<IndexPage>(ctx, page, _fs)
            {
                Metadata = new IndexPage(
                    posts.ToArray(),
                    null,
                    null,
                    new PageInfo(0, "---"))
            };
            return await engine.CompileRenderAsync(doc.File.Normalized(_fs), doc);
        }
    }
}