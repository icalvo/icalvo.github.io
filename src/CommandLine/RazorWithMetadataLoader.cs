﻿using System.Diagnostics;
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

    public async Task<SiteContents> Load(SiteContents siteContent, AbsolutePathEx projectRoot)
    {
        var sw = Stopwatch.StartNew();
        var postsPath = projectRoot / _contentDir;

        var files = Directory.GetFiles(postsPath.Normalized(), $"*{RazorExtension}", new EnumerationOptions { RecurseSubdirectories = _recursive })
            .Select(AbsolutePathEx.Create)
            .Where(f => !f.FileName.StartsWith('_'));
        
        var engine = _razorEngineFactory.Create(projectRoot.Normalized());
        foreach (var inputFile in files)
        {
            var dirPart = inputFile.Parent.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");
            var postFile = inputFile.RelativeTo(projectRoot) ?? throw new Exception("Should not happen");
            
            var doc = new Document<TMetadata>(siteContent, postFile);
            var fakeMetadata = doc.Metadata;

            var content = await engine.RenderWithoutLayout(inputFile, postFile, doc);

            doc.Content = content;
            if (ReferenceEquals(fakeMetadata, doc.Metadata))
            {
                throw new Exception(
                    $$"""You must set the metadata in your template, e.g. @{ Model.Metadata = new {{typeof(TMetadata).Name}} { Title = "My Title" }; }""");
            }

            if (doc.Metadata is Page page)
            {
                doc.OutputFile = page.Link;
            }
            else
            {
                string[] subDirectory = doc.Metadata is ISubDirectory s ? s.SubDirectory() : [];
            
                doc.OutputFile = new RelativePathEx([
                    ..dirPart,
                    ..subDirectory,
                    inputFile.Parts[^1].ReplaceEnd(RazorExtension, ".html")!]);
            }

            Logger.Info($"Loaded {doc.File} in {sw.ElapsedMilliseconds}ms");
        }

        siteContent.Add(new PostConfig(DisableLiveRefresh:false));
        return siteContent;
    }
}