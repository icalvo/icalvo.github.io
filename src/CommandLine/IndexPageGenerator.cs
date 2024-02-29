using SWGen;

namespace CommandLine;

public class IndexPageGenerator : MultipleStringGenerator
{
    private readonly IRazorEngineFactory _razorEngineFactory;

    public IndexPageGenerator(IRazorEngineFactory razorEngineFactory)
    {
        _razorEngineFactory = razorEngineFactory;
    }

    protected override IEnumerable<(RelativePathEx, Func<Task<string>>)> GenerateString(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, ISwgLogger logger, CancellationToken ct)
    {
        var engine = _razorEngineFactory.Create(projectRoot.Normalized());
        var batches = ctx.TryGetValues<Document<Post>>().OrderByDescending(p => p.Metadata.Published)
            .Batch(ctx.TryGetValue<SiteInfo>()?.PostPageSize ?? 5)
            .ToArray();
        return batches
            .Select(
                (batch, i) =>
                {
                    return ((RelativePathEx, Func<Task<string>>))(LinkFor(i)!, Func);

                    async Task<string> Func()
                    {
                        string? nextPageLink = LinkFor(i + 1);
                        string? prevPageLink = LinkFor(i - 1);

                        var doc = new Document<IndexPage>(ctx, inputFile)
                        {
                            Metadata = new IndexPage(
                                batch.ToArray(),
                                nextPageLink is not null ? new PageInfo(i + 2, nextPageLink) : null,
                                prevPageLink is not null ? new PageInfo(i, prevPageLink) : null,
                                new PageInfo(i + 1, LinkFor(i + 1)!))
                        };
                        return await engine.CompileRenderAsync(doc.File.Normalized(), doc);
                    }

                    string? LinkFor(int idx)
                    {
                        if (idx < 0) return null;
                        if (idx > batches.Length - 1) return null;
                        if (idx == 0) return "index.html";
                        return $"page{idx + 1}.html";
                    }
                });
    }
}