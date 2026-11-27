using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RazorLight;
using SWGen.FileSystems;
using SWGen.Generators;

namespace SWGen.Razor.Atom;

public class AtomGenerator<TIndex, TPost>(IRazorLightEngine engine, IFileSystem fs) : StringGenerator
    where TIndex : class, ICreatable<TIndex>, IIndexBuilder<TIndex, TPost>
    where TPost : class, IDated, ICreatable<TPost>
{
    protected override (RelativePathEx, Func<Task<string>>) GenerateString(SiteContents ctx, AbsolutePathEx projectRoot,
        RelativePathEx page, ISwgLogger logger, CancellationToken ct)
    {
        return ("feed.xml", Func);

        async Task<string> Func()
        {
            var posts = ctx.TryGetValues<Document<TPost>>().OrderByDescending(p => p.Metadata.Published);

            var doc = new Document<TIndex>(ctx, page, fs)
            {
                Metadata = TIndex.CreateIndex(posts)
            };

            return await engine.CompileRenderAsync(doc.File.Normalized(fs), doc);
        }
    }
}
