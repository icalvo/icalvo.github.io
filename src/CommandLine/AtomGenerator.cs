﻿using SWGen;

namespace CommandLine;

public class AtomGenerator : StringGenerator
{
    private readonly IRazorEngineFactory _razorEngineFactory;

    public AtomGenerator(IRazorEngineFactory razorEngineFactory)
    {
        _razorEngineFactory = razorEngineFactory;
    }

    protected override (RelativePathEx, Func<Task<string>>) GenerateString(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx page, ISwgLogger logger, CancellationToken ct)
    {
        return ("feed.xml", Func);

        async Task<string> Func()
        {
            var engine = _razorEngineFactory.Create(projectRoot.Normalized());
            var posts = ctx.TryGetValues<Document<Post>>().OrderByDescending(p => p.Metadata.Published);
        
            var doc = new Document<IndexPage>(ctx, page)
            {
                Metadata = new IndexPage(
                    posts.ToArray(),
                    null,
                    null,
                    new PageInfo(0, "---"))
            };
            return await engine.CompileRenderAsync(doc.File.Normalized(), doc);
        }
    }
}