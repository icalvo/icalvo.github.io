using System.Diagnostics;
using Microsoft.AspNetCore.Components.Forms;
using RazorLight.Razor;
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

    public async Task<SiteContents> Load(SiteContents siteContents, AbsolutePathEx projectRoot, ISwgLogger loaderLogger,
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
            LoadDocument);

        siteContents.Add(new PostConfig(DisableLiveRefresh:false));
        return siteContents;

        async ValueTask LoadDocument(AbsolutePathEx inputFile, CancellationToken ct2)
        {
            var inputRelativeFile = inputFile.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");

            var fileLogger = loaderLogger.BeginScope(inputRelativeFile.Normalized());
            var existing = siteContents.TryGetValues<Document<TMetadata>>().FirstOrDefault(doc => doc.File == inputRelativeFile);

            bool reprocessing;
            Document<TMetadata> doc;
            if (existing != null)
            {
                if (!existing.HasPendingLinks)
                {
                    fileLogger.Info("Already loaded");
                    return;
                }

                doc = existing;
                fileLogger.Debug("Reprocessing");
                existing.PendingLinks.Clear();
                reprocessing = true;
            }
            else
            {
                doc = new Document<TMetadata>(siteContents, inputRelativeFile);
                siteContents.Add(doc);
                reprocessing = false;
            }

            var fakeMetadata = doc.Metadata;


            var templateKey = inputFile.Normalized();
            var content = await engine.CompileRenderWithoutLayout(templateKey, doc);
            var imports = _razorEngineFactory.Project != null ? await _razorEngineFactory.Project.GetImportsAsync(templateKey) : Enumerable.Empty<RazorLightProjectItem>();
            fileLogger.Debug($"Imports: {imports.Select(i => i.Key).StringJoin(", ")}");

            doc.Content = content;
            if (ReferenceEquals(fakeMetadata, doc.Metadata))
            {
                throw new Exception($$"""You must set the metadata in your template, e.g. @{ Model.Metadata = new {{typeof(TMetadata).Name}} { Title = "My Title" }; }""");
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

            if (reprocessing && doc.HasPendingLinks)
            {
                foreach (var pendingLink in doc.PendingLinks)
                {
                    fileLogger.Error($"Pending link: {pendingLink}");
                }

                throw new Exception("Reprocessing did not resolve all links");
            }

            fileLogger.Info("Loaded in {sw.ElapsedMilliseconds}ms");
        }
    }
}