using System.Diagnostics;
using SWGen;

namespace CommandLine;

public class RazorWithMetadataLoader<TMetadata> : ILoader where TMetadata : class, ICreatable<TMetadata>
{
    private readonly RelativePathEx _contentDir;
    private readonly bool _recursive;
    private readonly IRazorEngineFactory _razorEngineFactory;
    private const string RazorExtension = ".cshtml";
    public RazorWithMetadataLoader(RelativePathEx contentDir, bool recursive, IRazorEngineFactory razorEngineFactory)
    {
        _contentDir = contentDir;
        _recursive = recursive;
        _razorEngineFactory = razorEngineFactory;
    }

    public override string ToString() => $"RazorWithMetadataLoader<{typeof(TMetadata).Name}>(\"{_contentDir}\", {_recursive})";

    public async Task<SiteContents> Load(SiteContents siteContent, AbsolutePathEx projectRoot, ISwgLogger loaderLogger,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var postsPath = projectRoot / _contentDir;

        var files = Directory.GetFiles(postsPath.Normalized(), $"*{RazorExtension}", new EnumerationOptions { RecurseSubdirectories = _recursive })
            .Select(AbsolutePathEx.Create)
            .Where(f => !f.FileName.StartsWith('_'));
        
        var engine = _razorEngineFactory.Create(projectRoot.Normalized());
        await Parallel.ForEachAsync(
            files,
            ct,
            async (inputFile, ct2) =>
            {
                var inputRelativeFile = inputFile.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");

                var fileLogger = loaderLogger.BeginScope(inputRelativeFile.Normalized());
                
                var doc = new Document<TMetadata>(siteContent, inputRelativeFile);
                var fakeMetadata = doc.Metadata;

                var content = await engine.CompileRenderWithoutLayout(inputFile, doc);

                doc.Content = content;
                if (ReferenceEquals(fakeMetadata, doc.Metadata))
                {
                    throw new Exception(
                        $$"""You must set the metadata in your template, e.g. @{ Model.Metadata = new {{typeof(TMetadata).Name}} { Title = "My Title" }; }""");
                }

                if (doc.Metadata is ILink link)
                {
                    doc.OutputFile = link.BuildLink(doc);
                }
                else
                {
                    var dirPart = inputFile.Parent.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");
                    var fileName = inputFile.Parts[^1];
                    doc.OutputFile = dirPart.Combine(Path.ChangeExtension(fileName, ".html"));
                }

                fileLogger.Info("Loaded in {sw.ElapsedMilliseconds}ms");
            });

        siteContent.Add(new PostConfig(DisableLiveRefresh:false));
        return siteContent;
    }
}