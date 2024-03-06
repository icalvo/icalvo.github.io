using RazorLight;
using SWGen;
using SWGen.FileSystems;

namespace CommandLine;

public class RazorGenerator<T> : StringGenerator where T : class, ICreatable<T>
{
    private readonly IRazorLightEngine _engine;
    private readonly IFileSystem _fs;

    public RazorGenerator(IRazorLightEngine engine, IFileSystem fs)
    {
        _engine = engine;
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
            TemplateLayoutToggle.IsEnabled = true;
        
            AbsolutePathEx pageAbsolutePath = projectRoot / page;
            string? layout = null;
            if (await pageAbsolutePath.Parent.GetFirstExistingFileInParentDirs(_fs, "_ViewStart.cshtml") is { } f)
            {
                logger.Info($"Running {f}...");
                var template = await _engine.CompileTemplateAsync(f.Normalized(_fs));
                _ = await _engine.RenderTemplateAsync(template, doc);
                layout = template.Layout;
            }

            return (await _engine.CompileRenderWithLayout(pageAbsolutePath.Normalized(_fs), doc, layout)).Tidy();            
        }
    }
}