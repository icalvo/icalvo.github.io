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

    protected override (RelativePathEx, string) GenerateString(SiteContents ctx, AbsolutePathEx projectRoot, RelativePathEx page)
    {
        var engine = _razorEngineFactory.Create(projectRoot.Normalized());
        var doc =
            ctx.TryGetValues<Document<T>>().SingleOrDefault(doc => doc.File == page)
            ?? throw new Exception($"{typeof(T).Name} {page} has not been loaded");

        TemplateLayoutToggle.IsEnabled = true;
        
        AbsolutePathEx pageAbsolutePath = projectRoot / page;
        string? layout = null;
        if (pageAbsolutePath.Parent.GetFirstExistingFileInParentDirs("_ViewStart.cshtml") is { } f)
        {
            Logger.Info($"Running {f}...");
            var template = engine.CompileTemplateAsync(f.Normalized()).Result;
            engine.RenderTemplateAsync(template, doc);
            _ = engine.CompileRenderAsync(f.Normalized(), doc).Result;
            layout = template.Layout;
        }

        var templateContent = File.ReadAllText(pageAbsolutePath.Normalized());
        if (!templateContent.Contains("<!--start-->"))
        {
            var tpl = engine.CompileTemplateAsync(page.Normalized()).Result;
            tpl.Layout = layout;
            var result = engine.RenderTemplateAsync(tpl, doc).Result;
            return (doc.OutputFile, result);
        }

        var fullTemplate = $"""
                            {templateContent.CutFromLine("<!--start-->")}

                            {doc.Content}
                            """;

        _razorEngineFactory.Project?.RegisterGenerationContent(page + ".generation", fullTemplate);
        
        var tpl2 = engine.CompileTemplateAsync(page + ".generation").Result;
        tpl2.Layout = layout;
        var fullContent = engine.RenderTemplateAsync(tpl2, doc).Result;

        return (doc.OutputFile, fullContent);
    }
}