namespace SWGen;

public interface IDocument
{
    PathEx File { get; }
    Uri Link { get; }
    SiteContents SiteContents { get; init; }
    SiteInfo SiteInfo { get; }
    object Metadata { get; set; }
    string? Title { get; }
    string Author { get; }
}