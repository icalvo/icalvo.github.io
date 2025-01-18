using SWGen.FileSystems;

namespace SWGen;

public class Document : IDocument
{
    private string? _content;

    public Document(SiteContents siteContents, RelativePathEx file, IFileSystem fs)
    {
        File = file;
        Fs = fs;
        OutputFile = RelativePathEx.Create("NOOUTPUT!!");
        SiteContents = siteContents;
        ((IDocument)this).Metadata = new object();
    }

    public RelativePathEx File { get; }

    public AbsolutePathEx AbsoluteFile => SiteInfo.ProjectRoot / File;

    public Task<string> ReadAssociatedFileAsync(string extension)
        => Fs.File.ReadAllTextAsync(AbsoluteFile.ReplaceExtension(extension));
    public RelativePathEx OutputFile { get; set; }
    public Uri RootRelativeLink => OutputFile.Url();
    public SiteContents SiteContents { get; init; }
    public SiteInfo SiteInfo => SiteContents.TryGetValue<SiteInfo>() ?? throw new Exception("SiteInfo not found!");
    object IDocument.Metadata { get; set; }
    protected IDocument This => this;
    public string? Title => (This.Metadata as ITitled)?.Title;
    public string Author => (This.Metadata as IAuthored)?.Author ?? SiteInfo?.Owner.Name ?? "Anonymous";
    public bool HasPendingLinks => PendingLinks.Count > 0;
    public List<string> PendingLinks { get; } = [];
    public Uri CanonicalLink => new (SiteInfo.Url, RootRelativeLink);
    
    public string Content
    {
        get
        {
            if (SiteContents.ContentAvailable)
            {
                return _content!;
            }

            throw new Exception($"Content is not available for {File} while loading!");
        }
        set => _content = value;
    }

    public string Summary => Content.UntilLine("<!--more-->");

    public IFileSystem Fs { get; }
}

public class Document<TMetadata> : Document where TMetadata : class, ICreatable<TMetadata>
{
    public Document(SiteContents siteContents, RelativePathEx file, IFileSystem fs) : base(siteContents, file, fs)
    {
        Metadata = TMetadata.Create();
    }

    public TMetadata Metadata
    {
        get => (TMetadata)This.Metadata;
        set => This.Metadata = value;
    }
}
