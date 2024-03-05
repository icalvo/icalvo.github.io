using SWGen;
using SWGen.FileSystems;

namespace CommandLine;

public class RazorGenerator<T> : StringGenerator where T : class, ICreatable<T>
{
    private readonly IRazorEngineFactory _razorEngineFactory;
    private readonly IFileSystem _fs;

    public RazorGenerator(IRazorEngineFactory razorEngineFactory, IFileSystem fs)
    {
        _razorEngineFactory = razorEngineFactory;
        _fs = fs;
    }

    protected override (RelativePathEx Link, Func<Task<string>> Content) GenerateString(SiteContents ctx,
        AbsolutePathEx projectRoot, RelativePathEx page, ISwgLogger logger, CancellationToken ct)
    {
        Document<T> doc =
            ctx.TryGetValues<Document<T>>().SingleOrDefault(doc => doc.File == page)
            ?? throw new Exception($"{typeof(T).Name} {page} has not been loaded");

        return (doc.OutputFile, Func);

        async Task<string> Func()
        {
            var engine = _razorEngineFactory.Create(projectRoot.Normalized(_fs));

            TemplateLayoutToggle.IsEnabled = true;
        
            AbsolutePathEx pageAbsolutePath = projectRoot / page;
            string? layout = null;
            if (await pageAbsolutePath.Parent.GetFirstExistingFileInParentDirs(_fs, "_ViewStart.cshtml") is { } f)
            {
                logger.Info($"Running {f}...");
                var template = await engine.CompileTemplateAsync(f.Normalized(_fs));
                _ = await engine.RenderTemplateAsync(template, doc);
                layout = template.Layout;
            }

            return (await engine.CompileRenderWithLayout(pageAbsolutePath.Normalized(_fs), doc, layout)).Tidy();            
        }
    }
}