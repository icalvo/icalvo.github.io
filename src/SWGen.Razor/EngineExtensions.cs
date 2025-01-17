using System;
using System.Threading.Tasks;
using RazorLight;

namespace SWGen.Razor;

public static class EngineExtensions
{
    public static Task<string> CompileRenderWithoutLayout<TMetadata>(this IRazorLightEngine engine,
        string templateKey, Document<TMetadata> doc)
        where TMetadata : class, ICreatable<TMetadata> =>
        engine.CompileRenderLayoutToggle(templateKey, doc, false);

    public static Task<string> CompileRenderWithLayout<TMetadata>(this IRazorLightEngine engine,
        string templateKey, Document<TMetadata> doc, string? layout)
        where TMetadata : class, ICreatable<TMetadata> =>
        engine.CompileRenderLayoutToggle(templateKey, doc, true, layout);
    
    public static async Task<string> CompileRenderLayoutToggle<TMetadata>(
        this IRazorLightEngine engine,
        string templateKey,
        Document<TMetadata> doc,
        bool withLayout,
        string? layout = null) where TMetadata : class, ICreatable<TMetadata>
    {
        var templatePage = await engine.CompileTemplateAsync(templateKey);
        if (withLayout)
        {
            templatePage.Layout = layout;
        }
        else
        {
            if (templatePage is ILayoutToggle lp)
            {
                lp.LayoutEnabled = false;
            }
            else
            {
                throw new Exception("Cannot disable layout unless your template page class implements ILayoutToggle");
            }
        }

        var content = await engine.RenderTemplateAsync(templatePage, doc);
        return content;
    }
}