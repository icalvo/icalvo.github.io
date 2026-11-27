using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RazorLight;
using SWGen.FileSystems;
using SWGen.Generators;

namespace SWGen.Razor;

public class RazorGenerator<T>(IRazorLightEngine engine, IFileSystem fs, Func<string, string> postRenderTransforms)
    : StringGenerator where T : class, ICreatable<T>
{
    protected override (RelativePathEx Link, Func<Task<string>> Content) GenerateString(SiteContents ctx,
        AbsolutePathEx projectRoot, RelativePathEx page, ISwgLogger logger, CancellationToken ct)
    {
        Document<T> doc =
            ctx.TryGetValues<Document<T>>().SingleOrDefault(doc => doc.File == page)
            ?? throw new InvalidOperationException($"{typeof(T).Name} {page} has not been loaded");

        return (doc.OutputFile, Func);

        async Task<string> Func()
        {
            TemplateLayoutToggle.IsEnabled = true;

            AbsolutePathEx pageAbsolutePath = projectRoot / page;
            string? layout = null;
            if (await pageAbsolutePath.Parent.GetFirstExistingFileInParentDirs(fs, "_ViewStart.cshtml") is { } f)
            {
                logger.Info($"Running {f}...");
                var template = await engine.CompileTemplateAsync(f.Normalized(fs));
                _ = await engine.RenderTemplateAsync(template, doc);
                layout = template.Layout;
            }

            var renderedContent = await engine.CompileRenderWithLayout(pageAbsolutePath.Normalized(fs), doc, layout);
            renderedContent = postRenderTransforms(renderedContent);
            return renderedContent;
        }
    }
}
