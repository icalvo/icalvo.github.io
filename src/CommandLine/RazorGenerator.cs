using RazorLight;
using SWGen;

namespace CommandLine;

public class RazorGenerator<T> : StringGenerator where T : class, ICreatable<T>
{
    private readonly IRazorEngineFactory _razorEngineFactory;

    public RazorGenerator(IRazorEngineFactory razorEngineFactory)
    {
        _razorEngineFactory = razorEngineFactory;
    }

    protected override (RelativePathEx Link, Func<Task<string>> Content) GenerateString(SiteContents ctx,
        AbsolutePathEx projectRoot, RelativePathEx page, CancellationToken ct)
    {
        Document<T> doc =
            ctx.TryGetValues<Document<T>>().SingleOrDefault(doc => doc.File == page)
            ?? throw new Exception($"{typeof(T).Name} {page} has not been loaded");

        return (doc.OutputFile, Func);

        async Task<string> Func()
        {
            var engine = _razorEngineFactory.Create(projectRoot.Normalized());

            TemplateLayoutToggle.IsEnabled = true;
        
            AbsolutePathEx pageAbsolutePath = projectRoot / page;
            string? layout = null;
            if (pageAbsolutePath.Parent.GetFirstExistingFileInParentDirs("_ViewStart.cshtml") is { } f)
            {
                Logger.Info($"Running {f}...");
                var template = await engine.CompileTemplateAsync(f.Normalized());
                _ = await engine.RenderTemplateAsync(template, doc);
                layout = template.Layout;
            }

            return await engine.RenderWithLayout(pageAbsolutePath, doc, layout);            
        }
    }
}