using RazorLight;
using SWGen;
using SWGen.FileSystems;

namespace CommandLine;

public class AtomGenerator : StringGenerator
{
    private readonly IRazorLightEngine _engine;
    private readonly IFileSystem _fs;

    public AtomGenerator(IRazorLightEngine engine, IFileSystem fs)
    {
        _engine = engine;
        _fs = fs;
    }

    protected override (RelativePathEx, Func<Task<string>>) GenerateString(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx page, ISwgLogger logger, CancellationToken ct)
    {
        return ("feed.xml", Func);

        async Task<string> Func()
        {
            var posts = ctx.TryGetValues<Document<Post>>().OrderByDescending(p => p.Metadata.Published);
        
            var doc = new Document<IndexPage>(ctx, page, _fs)
            {
                Metadata = new IndexPage(
                    posts.ToArray(),
                    null,
                    null,
                    new PageInfo(0, "---"))
            };
            return await _engine.CompileRenderAsync(doc.File.Normalized(_fs), doc);
        }
    }
}