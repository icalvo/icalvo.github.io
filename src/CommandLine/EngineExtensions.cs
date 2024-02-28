using Markdig;
using Markdig.Syntax;
using RazorLight;
using SWGen;

namespace CommandLine;

public static class EngineExtensions
{
    public static Task<string> RenderWithoutLayout<TMetadata>(this IRazorLightEngine engine,
        AbsolutePathEx inputFile, Document<TMetadata> doc)
        where TMetadata : class, ICreatable<TMetadata> =>
        engine.RenderEx(inputFile, doc, false);

    public static Task<string> RenderWithLayout<TMetadata>(this IRazorLightEngine engine,
        AbsolutePathEx inputFile, Document<TMetadata> doc, string? layout)
        where TMetadata : class, ICreatable<TMetadata> =>
        engine.RenderEx(inputFile, doc, true, layout);
    
    public static async Task<string> RenderEx<TMetadata>(
        this IRazorLightEngine engine,
        AbsolutePathEx inputFile,
        Document<TMetadata> doc,
        bool withLayout,
        string? layout = null) where TMetadata : class, ICreatable<TMetadata>
    {
        var templatePage = await engine.CompileTemplateAsync(inputFile.Normalized());
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
                throw new Exception("Cannot disable layout unless your template page class ILayoutToggle");
            }
        }

        var content = await engine.RenderTemplateAsync(templatePage, doc);
        content = StringManipulation.TransformTags(content, "markdown", s => Markdown.Parse(s, trackTrivia: true).ToHtml());

        return content;
    }
}