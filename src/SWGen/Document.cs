namespace SWGen;

public class Document : IDocument
{
    private string? _content;

    public Document(SiteContents siteContents, RelativePathEx file)
    {
        File = file;
        OutputFile = RelativePathEx.Create("NOOUTPUT!!");
        SiteContents = siteContents;
        ((IDocument)this).Metadata = new object();
    }

    public RelativePathEx File { get; }
    public RelativePathEx OutputFile { get; set; }
    public Uri RootRelativeLink => OutputFile.Url();
    public SiteContents SiteContents { get; init; }
    public SiteInfo SiteInfo => SiteContents.TryGetValue<SiteInfo>() ?? throw new Exception("SiteInfo not found!");
    object IDocument.Metadata { get; set; }
    protected IDocument This => this;
    public string? Title => (This.Metadata as ITitled)?.Title;
    public string Author => (This.Metadata as IAuthored)?.Author ?? SiteInfo?.Owner.Name ?? "Anonymous";
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
}

public class Document<TMetadata> : Document where TMetadata : class, ICreatable<TMetadata>
{
    public Document(SiteContents siteContents, RelativePathEx file) : base(siteContents, file)
    {
        Metadata = TMetadata.Create();
    }

    public TMetadata Metadata
    {
        get => (TMetadata)This.Metadata;
        set => This.Metadata = value;
    }
}
