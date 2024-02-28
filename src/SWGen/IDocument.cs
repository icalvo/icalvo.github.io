namespace SWGen;

public interface IDocument
{
    RelativePathEx File { get; }
    Uri CanonicalLink { get; }
    Uri RootRelativeLink { get; }
    SiteContents SiteContents { get; init; }
    SiteInfo SiteInfo { get; }
    object Metadata { get; set; }
    string? Title { get; }
    string Author { get; }
}