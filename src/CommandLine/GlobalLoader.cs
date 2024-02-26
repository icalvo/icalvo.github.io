using SWGen;

namespace CommandLine;

public class GlobalLoader : ILoader
{
    public Task<SiteContents> Load(SiteContents siteContent, AbsolutePathEx projectRoot)
    {
        siteContent.Add(new SiteInfo
        {
            Title = "Ignacio Calvo Blog",
            Owner = new Author
            {
                Name = "Ignacio Calvo Martínez",
                Email = "ignacio.calvo@gmail.com"
            },
            Description = "Professional developer and amateur musician.",
            PostPageSize = 5,
            GoogleSiteVerification = "lYI_fzae0p3Hx_Ta5lkgLAztpNDFaWHb39crE9KBYjA",
            GoogleAnalytics = "UA-1520724-2",
            SocialLinks = new()
            {
                { "twitter", "ignaciocalvo" },
                { "github", " icalvo" },
                { "linkedin", "ignaciocalvomartinez" },
                { "youtube", "icalvo" },
                { "instagram", "ignaciocalvo2" },
                { "facebook", "ignacio.calvomartinez" },
                { "rss", "RSS feed" },
            },
            BaseUrl = "",
            Url = "https://ignaciocalvo.com",
            ProjectRoot = projectRoot,
        });
        
        return Task.FromResult(siteContent);
    }
}