using Microsoft.CodeAnalysis;

namespace SWGen;

public class Document : IDocument
{
    public Document(SiteContents siteContents, PathEx file)
    {
        File = file;
        OutputFile = "NOOUTPUT!!";
        SiteContents = siteContents;
        ((IDocument)this).Metadata = new object();
    }

    public PathEx File { get; }
    public RelativePathEx OutputFile { get; set; }
    public Uri Link => OutputFile.Url();
    public SiteContents SiteContents { get; init; }
    public SiteInfo SiteInfo => SiteContents.TryGetValue<SiteInfo>() ?? throw new Exception("SiteInfo not found!");

    object IDocument.Metadata { get; set; }

    protected IDocument This => this;
    public string? Title => (This.Metadata as ITitled)?.Title;
    public string Author => (This.Metadata as IAuthored)?.Author ?? SiteInfo?.Owner.Name ?? "Anonymous";
}

public class Document<TMetadata> : Document where TMetadata : class, ICreatable<TMetadata>
{
    private string? _content;

    public Document(SiteContents siteContents, PathEx file) : base(siteContents, file)
    {
        Metadata = TMetadata.Create();
        SiteContents.Add(this);
    }

    public TMetadata Metadata
    {
        get => (TMetadata)This.Metadata;
        set => This.Metadata = value;
    }

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