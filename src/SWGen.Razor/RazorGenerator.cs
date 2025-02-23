using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RazorLight;
using SWGen.FileSystems;
using SWGen.Generators;

namespace SWGen.Razor;

public class RazorGenerator<T> : StringGenerator where T : class, ICreatable<T>
{
    private readonly IRazorLightEngine _engine;
    private readonly IFileSystem _fs;
    private readonly Func<string, string> _postRenderTransforms;

    public RazorGenerator(IRazorLightEngine engine, IFileSystem fs, Func<string, string> postRenderTransforms)
    {
        _engine = engine;
        _fs = fs;
        _postRenderTransforms = postRenderTransforms;
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

            var renderedContent = await _engine.CompileRenderWithLayout(pageAbsolutePath.Normalized(_fs), doc, layout);
            renderedContent = _postRenderTransforms(renderedContent);
            return renderedContent;            
        }
    }
}