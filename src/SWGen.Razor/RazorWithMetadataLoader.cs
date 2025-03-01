﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RazorLight;
using RazorLight.Razor;
using SWGen.FileSystems;

namespace SWGen.Razor;

public class RazorWithMetadataLoader<TMetadata> : ILoader where TMetadata : class, ICreatable<TMetadata>
{
    private readonly RelativePathEx _contentDir;
    private readonly bool _recursive;
    private readonly IRazorLightEngine _engine;
    private readonly RazorLightProject _project;
    private readonly IFileSystem _fs;
    private const string RazorExtension = ".cshtml";

    public RazorWithMetadataLoader(
        RelativePathEx contentDir,
        bool recursive,
        IRazorLightEngine engine,
        IFileSystem fs,
        RazorLightProject project)
    {
        _contentDir = contentDir;
        _recursive = recursive;
        _engine = engine;
        _fs = fs;
        _project = project;
    }

    public override string ToString() => $"RazorWithMetadataLoader<{typeof(TMetadata).Name}>(\"{_contentDir}\", rec:{_recursive})";

    public async Task<LoaderResult> Load(SiteContents siteContents, AbsolutePathEx projectRoot, ISwgLogger loaderLogger,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var contentPath = projectRoot / _contentDir;
        var files = _fs.Directory.GetFiles(contentPath, $"*{RazorExtension}", new EnumerationOptions { RecurseSubdirectories = _recursive })
            .Where(f => !f.FileName.StartsWith('_'));

        bool completed = true;
        await Parallel.ForEachAsync(files, ct, LoadDocument);

        return new (siteContents, completed);

        async ValueTask LoadDocument(AbsolutePathEx inputFile, CancellationToken ct2)
        {
            var inputRelativeFile = inputFile.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");

            var fileLogger = loaderLogger.BeginScope(inputRelativeFile.Normalized(_fs));
            var existing = siteContents.TryGetValues<Document<TMetadata>>().FirstOrDefault(doc => doc.File == inputRelativeFile);

            bool reprocessing;
            Document<TMetadata> doc;
            if (existing != null)
            {
                if (!existing.HasPendingLinks)
                {
                    fileLogger.Debug("Already loaded");
                    return;
                }

                doc = existing;
                fileLogger.Debug("Reprocessing");
                existing.PendingLinks.Clear();
                reprocessing = true;
            }
            else
            {
                doc = new Document<TMetadata>(siteContents, inputRelativeFile, _fs);
                siteContents.Add(doc);
                reprocessing = false;
            }

            var fakeMetadata = doc.Metadata;

            var templateKey = inputFile.Normalized(_fs);
            var content = await _engine.CompileRenderWithoutLayout(templateKey, doc);
            var imports = await _project.GetImportsAsync(templateKey);
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

            if (doc.HasPendingLinks)
            {
                if (reprocessing)
                {
                    foreach (var pendingLink in doc.PendingLinks)
                    {
                        fileLogger.Error($"Pending link: {pendingLink}");
                    }

                    throw new Exception("Reprocessing did not resolve all links");
                }

                completed = false;
            }

            fileLogger.Info($"Loaded in {sw.ElapsedMilliseconds}ms");
        }
    }
}