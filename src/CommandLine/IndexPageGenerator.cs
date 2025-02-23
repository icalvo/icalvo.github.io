using RazorLight;
using SWGen;
using SWGen.FileSystems;
using SWGen.Generators;

namespace CommandLine;

public class IndexPageRazorGenerator : MultipleStringGenerator
{
    private const string FirstPageLink = "index.html";
    private readonly IRazorLightEngine _engine;
    private readonly IFileSystem _fs;
    private readonly Func<string, string> _postRenderTransforms;

    private string OtherPageLink(int number) => $"page{number}.html";

    public IndexPageRazorGenerator(IRazorLightEngine engine, IFileSystem fs, Func<string, string> postRenderTransforms)
    {
        _engine = engine;
        _fs = fs;
        _postRenderTransforms = postRenderTransforms;
    }

    protected override IEnumerable<(RelativePathEx, Func<Task<string>>)> GenerateStrings(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx inputFile, ISwgLogger logger, CancellationToken ct)
    {
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
                        var batchArray = batch.ToArray();
                        foreach (var item in batchArray)
                        {
                            item.Content = _postRenderTransforms(item.Content);
                        }

                        var doc = new Document<IndexPage>(ctx, inputFile, _fs)
                        {
                            OutputFile = UnsafeLinkFor(pageNumber),
                            Metadata = new IndexPage(
                                batchArray,
                                IsValid(pageNumber + 1) ? new PageInfo(pageNumber + 1, UnsafeLinkFor(pageNumber + 1)) : null,
                                IsValid(pageNumber - 1) ? new PageInfo(pageNumber - 1, UnsafeLinkFor(pageNumber - 1)) : null,
                                new PageInfo(pageNumber, UnsafeLinkFor(pageNumber)))
                        };
                        return (await _engine.CompileRenderAsync(doc.File.Normalized(_fs), doc));
                    }

                    bool IsValid(int number) => number > 0 && number <= batches.Length;

                    string UnsafeLinkFor(int number) => number == 1 ? FirstPageLink : OtherPageLink(number);
                });
    }
}
