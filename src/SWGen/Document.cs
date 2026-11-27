using SWGen.FileSystems;

namespace SWGen;

public class Document(SiteContents siteContents, RelativePathEx file, IFileSystem fs) : IDocument
{
    private string? _content;

    public RelativePathEx File { get; } = file;

    public AbsolutePathEx AbsoluteFile => SiteInfo.ProjectRoot / File;

    public Task<string> ReadAssociatedFileAsync(string extension)
        => Fs.File.ReadAllTextAsync(AbsoluteFile.ReplaceExtension(extension));
    public RelativePathEx OutputFile { get; set; } = RelativePathEx.Create("NOOUTPUT!!");
    public Uri RootRelativeLink => OutputFile.Url();
    public SiteContents SiteContents { get; init; } = siteContents;
    public SiteInfo SiteInfo => SiteContents.TryGetValue<SiteInfo>()
        ?? throw new InvalidOperationException("SiteInfo not found!");
    object IDocument.Metadata { get; set; } = new();
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
            if (!SiteContents.ContentAvailable)
            {
                throw new InvalidOperationException($"Content is not available for {File} while loading!");
            }

            return _content
                ?? throw new InvalidOperationException($"Content was not set for {File}");
        }
        set => _content = value;
    }

    public string Summary => Content.UntilLine("<!--more-->");

    public IFileSystem Fs { get; } = fs;
}
