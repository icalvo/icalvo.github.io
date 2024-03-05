using SWGen;
using SWGen.FileSystems;

namespace CommandLine;

public class IndexPageGenerator : MultipleStringGenerator
{
    private readonly IRazorEngineFactory _razorEngineFactory;
    private readonly IFileSystem _fs;
    private const string FirstPageLink = "index.html";
    private string OtherPageLink(int number) => $"page{number}.html";

    public IndexPageGenerator(IRazorEngineFactory razorEngineFactory, IFileSystem fs)
    {
        _razorEngineFactory = razorEngineFactory;
        _fs = fs;
    }

    protected override IEnumerable<(RelativePathEx, Func<Task<string>>)> GenerateString(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, ISwgLogger logger, CancellationToken ct)
    {
        var engine = _razorEngineFactory.Create(projectRoot.Normalized(_fs));
        var batches = ctx.TryGetValues<Document<Post>>().OrderByDescending(p => p.Metadata.Published)
            .Batch(ctx.TryGetValue<SiteInfo>()?.PostPageSize ?? 5)
            .ToArray();
        return batches
            .Select(
                (batch, i) =>
                {
                    var pageNumber = i + 1;
                    return ((RelativePathEx, Func<Task<string>>))(UnsafeLinkFor(pageNumber), Content);

                    async Task<string> Content()
                    {
                        var doc = new Document<IndexPage>(ctx, inputFile, _fs)
                        {
                            OutputFile = UnsafeLinkFor(pageNumber),
                            Metadata = new IndexPage(
                                batch.ToArray(),
                                IsValid(pageNumber + 1) ? new PageInfo(pageNumber + 1, UnsafeLinkFor(pageNumber + 1)) : null,
                                IsValid(pageNumber - 1) ? new PageInfo(pageNumber - 1, UnsafeLinkFor(pageNumber - 1)) : null,
                                new PageInfo(pageNumber, UnsafeLinkFor(pageNumber)))
                        };
                        return (await engine.CompileRenderAsync(doc.File.Normalized(_fs), doc)).Tidy();
                    }

                    bool IsValid(int number) => number > 0 && number <= batches.Length;

                    string UnsafeLinkFor(int number) => number == 1 ? FirstPageLink : OtherPageLink(number);
                });
    }
}