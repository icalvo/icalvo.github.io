using System.Diagnostics;
using Markdig;
using SWGen;

namespace CommandLine;

public class RazorWithMetadataLoader<TMetadata> : ILoader where TMetadata : class, ICreatable<TMetadata>
{
    private readonly RelativePathEx _contentDir;
    private readonly bool _recursive;
    private readonly IRazorEngineFactory _razorEngineFactory;

    public RazorWithMetadataLoader(RelativePathEx contentDir, bool recursive, IRazorEngineFactory razorEngineFactory)
    {
        _contentDir = contentDir;
        _recursive = recursive;
        _razorEngineFactory = razorEngineFactory;
    }

    public async Task<SiteContents> Load(SiteContents siteContent, AbsolutePathEx projectRoot)
    {
        var sw = Stopwatch.StartNew();
        var postsPath = projectRoot / _contentDir;
        var engine = _razorEngineFactory.Create(projectRoot.Normalized());

        var files = Directory.GetFiles(postsPath.Normalized(), "*.cshtml", new EnumerationOptions { RecurseSubdirectories = _recursive })
            .Select(AbsolutePathEx.Create)
            .Where(f => !f.FileName.StartsWith('_'));
        
        foreach (var file in files)
        {
            var dirPart = file.Parent.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");
            var postFile = file.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");
            
            var doc = new Document<TMetadata>(siteContent, postFile);
            var fakeMetadata = doc.Metadata;
            //
            // Logger.Info("Running _ViewStart.cshtml");
            // if (file.Parent.GetFirstExistingFileInParentDirs("_ViewStart.cshtml") is { } f)
            // {
            //     await engine.CompileRenderAsync(f.Normalized(), doc);
            // }
            //

            var templatePage = await engine.CompileTemplateAsync(postFile.Normalized());
            if (templatePage is ILayoutToggle lp)
            {
                lp.LayoutEnabled = false;
            }
            // else
            //     throw new Exception();

            var content = await engine.RenderTemplateAsync(templatePage, doc);

            var extension = ".cshtml";
            if (file.Parts[^1].EndsWith(".markdown.cshtml"))
            {
                var mdDoc = Markdown.Parse(content, trackTrivia: true);
                content = mdDoc.ToHtml();
                extension = ".markdown.cshtml";
            }

            doc.Content = content;
            if (ReferenceEquals(fakeMetadata, doc.Metadata))
            {
                throw new Exception(
                    $$"""You must set the metadata in your template, e.g. @{ Model.Metadata = new {{typeof(TMetadata).Name}} { Title = "My Title" }; }""");
            }

            string[] subDirectory = doc.Metadata is ISubDirectory s ? s.SubDirectory() : [];

            doc.OutputFile = new RelativePathEx([
                ..dirPart,
                ..subDirectory,
                file.Parts[^1].ReplaceEnd(extension, ".html")!]);

            Logger.Info($"Loaded {doc.File} in {sw.ElapsedMilliseconds}ms");
        }

        siteContent.Add(new PostConfig(DisableLiveRefresh:false));
        return siteContent;
    }
}