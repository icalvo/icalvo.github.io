using SWGen.FileSystems;

namespace SWGen;

public class SiteInfo
{
    public required string Title { get; init; }
    public required int PostPageSize { get; init; }
    public required string Description { get; init; }
    public required Author Owner { get; init;  }
    public required Dictionary<string, string> SocialLinks { get; init; }
    public required string GoogleAnalytics { get; init; }
    public required string GoogleSiteVerification { get; init; }
    public required string BaseUrl { get; init; }
    public required Uri Url { get; init; }
    public required AbsolutePathEx ProjectRoot { get; init; }
    public required string DisqusShortName { get; init; }
}